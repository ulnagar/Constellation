namespace Constellation.Infrastructure.Jobs;

using Application.DTOs;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AttendanceReportJob : IAttendanceReportJob
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    private Guid JobId { get; set; }

    public AttendanceReportJob(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IMediator mediator,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _mediator = mediator;
        _logger = logger.ForContext<IAttendanceReportJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        JobId = jobId;

        DateOnly startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)).VerifyStartOfFortnight();
        DateOnly endDate = startDate.AddDays(12);

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
                Result<FileDto> studentReportRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(student.StudentId, startDate, endDate), cancellationToken);

                if (studentReportRequest.IsFailure)
                    continue;

                string tempFile = Path.GetTempFileName();
                await File.WriteAllBytesAsync(tempFile, studentReportRequest.Value.FileData, cancellationToken);
                studentFiles.Add(tempFile, studentReportRequest.Value.FileName);

                await SendParentEmail(studentReportRequest.Value, student, startDate, cancellationToken);
            }

            _logger.Information("{id}: Sending reports to school {school}", JobId, school.First().School.Name);

            // Email all the files to the school
            List<System.Net.Mail.Attachment> attachmentList = new();

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
                        await using StreamWriter streamWriter = new(zipArchiveEntry.Open());
                        byte[] fileData = await File.ReadAllBytesAsync(file.Key, cancellationToken);
                        await streamWriter.BaseStream.WriteAsync(fileData, 0, fileData.Length, cancellationToken);
                    }
                }

                MemoryStream attachmentStream = new(memoryStream.ToArray());

                attachmentList.Add(new(attachmentStream, "Attendance Reports.zip", MediaTypeNames.Application.Zip));
            }
            else
            {
                foreach (KeyValuePair<string, string> file in studentFiles)
                {
                    byte[] fileData = await File.ReadAllBytesAsync(file.Key, cancellationToken);

                    attachmentList.Add(new(new MemoryStream(fileData), file.Value, MediaTypeNames.Application.Pdf));
                }
            }

            // Email the file to the school contacts
            await SendSchoolEmailAsync(school.Key, attachmentList, startDate, cancellationToken);

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
        FileDto file,
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
            MemoryStream stream = new MemoryStream(file.FileData);

            bool success = await _emailService.SendParentAttendanceReportEmail(
                student.DisplayName, 
                dateToReport, 
                dateToReport.AddDays(12), 
                recipients, 
                new List<Attachment> { new(stream, file.FileName) }, 
                cancellationToken);

            if (success)
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Information("{id}: Message sent via Email to {parent} ({email}) with attachment: {filename}", JobId, recipient.Name, recipient.Email, file.FileName);
            }
            else
            {
                foreach (EmailRecipient recipient in recipients)
                    _logger.Warning("{id}: FAILED to send email to {parent} ({email}) with attachment: {filename}", JobId, recipient.Name, recipient.Email, file.FileName);
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
}
