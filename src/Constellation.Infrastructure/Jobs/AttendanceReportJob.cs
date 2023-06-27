namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Templates.Views.Documents.Attendance;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class AttendanceReportJob : IAttendanceReportJob, IHangfireJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IOfferingSessionsRepository _sessionRepository;
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ICourseOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IEmailService _emailService;
    private readonly IPDFService _pdfService;
    private readonly IRazorViewToStringRenderer _renderService;
    private readonly ISentralGateway _sentralGateway;
    private readonly ILogger _logger;

    private Guid JobId { get; set; }

    public AttendanceReportJob(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IOfferingSessionsRepository sessionRepository,
        ITimetablePeriodRepository periodRepository,
        ICourseOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IEmailService emailService,
        IPDFService pdfService, 
        IRazorViewToStringRenderer renderService,
        ISentralGateway sentralGateway, 
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _sessionRepository = sessionRepository;
        _periodRepository = periodRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _emailService = emailService;
        _pdfService = pdfService;
        _renderService = renderService;
        _sentralGateway = sentralGateway;
        _logger = logger.ForContext<IAttendanceReportJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        JobId = jobId;

        DateOnly dateToReport = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)).VerifyStartOfFortnight();

        List<Student> students = await _studentRepository.GetCurrentStudentsWithSchool(cancellationToken);
        List<IGrouping<string, Student>> studentsBySchool = students
            .OrderBy(s => s.School.Name)
            .GroupBy(s => s.SchoolCode)
            .ToList();

        foreach (IGrouping<string, Student> school in studentsBySchool)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _logger.Information("{id}: Processing School: {name}", JobId, school.First().School.Name);

            Dictionary<string, string> studentFiles = new();

            foreach (Student student in school)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                _logger.Information("{id}: Creating Report for {name}", JobId, student.DisplayName);
                // Get Data from server
                var definition = new { header = "", body = "" };

                (string,string) data = await GetStudentData(student, dateToReport, cancellationToken);

                // Save to a temporary file
                MemoryStream pdfStream = _pdfService
                    .StringToPdfStream(data.Item2, data.Item1);

                string tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, pdfStream.ToArray());
                string filename = $"{student.LastName}, {student.FirstName} - {dateToReport:yyyy-MM-dd} - Attendance Report.pdf";
                studentFiles.Add(tempFile, filename);

                await SendParentEmail(pdfStream, filename, student, dateToReport, cancellationToken);
            }

            _logger.Information("{id}: Sending reports to school {school}", JobId, school.First().School.Name);

            // Email all the files to the school
            List<Attachment> attachmentList = new();

            if (cancellationToken.IsCancellationRequested)
                return;

            // Create ZIP file of all attachments, if # of attachments is greater than 4
            if (studentFiles.Count > 4)
            {
                using MemoryStream memoryStream = new();
                using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
                {
                    foreach (KeyValuePair<string, string> file in studentFiles)
                    {
                        ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.Value);
                        using StreamWriter streamWriter = new(zipArchiveEntry.Open());
                        byte[] fileData = File.ReadAllBytes(file.Key);
                        streamWriter.BaseStream.Write(fileData, 0, fileData.Length);
                    }
                }

                MemoryStream attachmentStream = new(memoryStream.ToArray());

                attachmentList.Add(new(attachmentStream, "Attendance Reports.zip", MediaTypeNames.Application.Zip));
            }
            else
            {
                foreach (KeyValuePair<string, string> file in studentFiles)
                {
                    byte[] fileData = File.ReadAllBytes(file.Key);

                    attachmentList.Add(new(new MemoryStream(fileData), file.Value, MediaTypeNames.Application.Pdf));
                }
            }

            // Email the file to the parents
            await SendSchoolEmailAsync(school.Key, attachmentList, dateToReport, cancellationToken);

            _logger.Information("{id}: Cleaning up temporary files created for {school}", JobId, school.First().School.Name);

            // Delete all temp files
            foreach (KeyValuePair<string, string> entry in studentFiles)
            {
                if (File.Exists(entry.Key))
                    File.Delete(entry.Key);
            }
        }
    }

    private async Task SendParentEmail(
        MemoryStream pdfStream, 
        string filename, 
        Student student, 
        DateOnly dateToReport, 
        CancellationToken cancellationToken)
    {
        // Email the file to the parents
        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);
        List<Parent> parents = families.SelectMany(family => family.Parents).ToList();
        List<EmailRecipient> recipients = new();

        foreach (Family family in families)
        {
            Result<EmailRecipient> result = EmailRecipient.Create(family.FamilyTitle, family.FamilyEmail);

            if (result.IsSuccess)
                recipients.Add(result.Value);
        }

        foreach (Parent parent in parents)
        {
            Result<Name> nameResult = Name.Create(parent.FirstName, string.Empty, parent.LastName);

            if (nameResult.IsFailure)
                continue;

            Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, parent.EmailAddress);

            if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                recipients.Add(result.Value);
        }

        if (recipients.Any())
        {
            bool success = await _emailService.SendParentAttendanceReportEmail(
                student.DisplayName, 
                dateToReport, 
                dateToReport.AddDays(12), 
                recipients, 
                new List<Attachment> { new(pdfStream, filename) }, 
                cancellationToken);

            if (success)
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: Message sent via Email to {parent} ({email}) with attachment: {filename}", JobId, recipient.Name, recipient.Email, filename);
            }
            else
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Warning("{id}: FAILED to send email to {parent} ({email}) with attachment: {filename}", JobId, recipient.Name, recipient.Email, filename);
            }
        }
        else
        {
            await _emailService.SendAdminAbsenceContactAlert(student.DisplayName);
        }
    }
    
    private async Task SendSchoolEmailAsync(
        string schoolCode, 
        List<Attachment> attachmentList, 
        DateOnly dateToReport, 
        CancellationToken cancellationToken)
    {
        List<SchoolContact> coordinators = await _contactRepository.GetBySchoolAndRole(schoolCode, SchoolContactRole.Coordinator, cancellationToken);

        List<EmailRecipient> recipients = new();

        foreach (SchoolContact coordinator in coordinators)
        {
            Result<Name> nameResult = Name.Create(coordinator.FirstName, string.Empty, coordinator.LastName);
            if (nameResult.IsFailure)
                continue;

            Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, coordinator.EmailAddress);

            if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                recipients.Add(result.Value);
        }

        if (recipients.Any())
        {
            bool success = await _emailService.SendSchoolAttendanceReportEmail(
               dateToReport,
               dateToReport.AddDays(12),
               recipients,
               attachmentList,
               cancellationToken);

            if (success)
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: Message sent via Email to {contact} ({email}) with Attendance Reports for {school}", JobId, recipient.Name, recipient.Email, schoolCode);
            }
            else
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Warning("{id}: FAILED to send email to {contact} ({email}) with Attendance Reports for {school}", JobId, recipient.Name, recipient.Email, schoolCode);
            }
        }
    }

    private async Task<ValueTuple<string, string>> GetStudentData(
        Student student, 
        DateOnly startDate,
        CancellationToken cancellationToken)
    {
        AttendanceReportViewModel viewModel = new()
        {
            StudentName = student.DisplayName,
            StartDate = startDate.VerifyStartOfFortnight(),
            ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(startDate.Year.ToString())
        };

        DateOnly endDate = viewModel.StartDate.AddDays(12);
        List<Absence> periodAbsences = await _absenceRepository.GetForStudentFromDateRange(student.StudentId, viewModel.StartDate, endDate, cancellationToken);

        foreach (var absence in periodAbsences)
        {
            List<Response> responses = await _responseRepository.GetAllForAbsence(absence.Id, cancellationToken);

            viewModel.Absences.Add(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence(absence, responses));
        }

        List<DateOnly> reportableDates = viewModel.StartDate.Range(endDate);

        foreach (DateOnly date in reportableDates)
        {
            AttendanceReportViewModel.DateSessions entry = new()
            {
                Date = date,
                DayNumber = date.GetDayNumber()
            };

            List<OfferingSession> sessions = await _sessionRepository.GetAllForStudentAndDayDuringTime(student.StudentId, entry.DayNumber, date);

            foreach (IGrouping<int, OfferingSession> offeringSessions in sessions.GroupBy(s => s.OfferingId))
            {
                CourseOffering offering = await _offeringRepository.GetById(offeringSessions.Key, cancellationToken);
                Course course = await _courseRepository.GetById(offering.CourseId, cancellationToken);

                List<TimetablePeriod> periods = await _periodRepository.GetForOfferingOnDay(offeringSessions.Key, date, entry.DayNumber, cancellationToken);
                TimetablePeriod firstPeriod = periods.First(period => period.StartTime == periods.Min(p => p.StartTime));
                TimetablePeriod lastPeriod = periods.First(period => period.EndTime == periods.Max(p => p.EndTime));
                
                if (periods.Count() == 1)
                {
                    entry.SessionsByOffering.Add(new()
                    {
                        PeriodName = periods.First().Name,
                        PeriodTimeframe = $"{firstPeriod.StartTime.As12HourTime()} - {lastPeriod.EndTime.As12HourTime()}",
                        OfferingName = offering.Name,
                        CourseName = course.Name,
                        OfferingId = offeringSessions.Key
                    });
                }
                else
                {
                    entry.SessionsByOffering.Add(new()
                    {
                        PeriodName = $"{firstPeriod.Name} - {lastPeriod.Name}",
                        PeriodTimeframe = $"{firstPeriod.StartTime.As12HourTime()} - {lastPeriod.EndTime.As12HourTime()}",
                        OfferingName = offering.Name,
                        CourseName = course.Name,
                        OfferingId = offeringSessions.Key
                    });
                }
            }

            viewModel.DateData.Add(entry);
        }

        string headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
        string htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

        return new (headerString, htmlString);
    }
}
