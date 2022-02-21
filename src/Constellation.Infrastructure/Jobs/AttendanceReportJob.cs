using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Presentation.Server.Areas.Reports.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class AttendanceReportJob : IAttendanceReportJob, IScopedService, IHangfireJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPDFService _pdfService;
        private readonly IRazorViewToStringRenderer _renderService;
        private readonly ISentralGateway _sentralGateway;
        private readonly ILogger<IAttendanceReportJob> _logger;

        public AttendanceReportJob(IUnitOfWork unitOfWork, IEmailService emailService,
            IPDFService pdfService, IRazorViewToStringRenderer renderService,
            ISentralGateway sentralGateway, ILogger<IAttendanceReportJob> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _pdfService = pdfService;
            _renderService = renderService;
            _sentralGateway = sentralGateway;
            _logger = logger;
        }

        public async Task StartJob()
        {
            var jobStatus = await _unitOfWork.JobActivations.GetForJob(nameof(IAttendanceReportJob));
            if (jobStatus == null || !jobStatus.IsActive)
            {
                _logger.LogWarning("Stopped due to job being set inactive.");
                return;
            }

            var dateToReport = DateTime.Today.AddDays(-1).VerifyStartOfFortnight();

            var students = await _unitOfWork.Students.AllActiveAsync();
            var studentsBySchool = students.GroupBy(s => s.SchoolCode).ToList();

            foreach (var school in studentsBySchool)
            {
                _logger.LogInformation($"Processing School: {school.First().School.Name}");

                var studentFiles = new Dictionary<string, string>();

                foreach (var student in school)
                {
                    _logger.LogInformation($" Creating Report for {student.DisplayName}");
                    // Get Data from server
                    var definition = new { header = "", body = "" };

                    var data = await GetStudentData(student, dateToReport);

                    // Save to a temporary file
                    var pdfStream = _pdfService.StringToPdfStream(data.Item2, data.Item1);
                    var tempFile = Path.GetTempFileName();
                    File.WriteAllBytes(tempFile, pdfStream.ToArray());
                    var filename = $"{student.LastName}, {student.FirstName} - {dateToReport:yyyy-MM-dd} - Attendance Report.pdf";
                    studentFiles.Add(tempFile, filename);

                    await SendParentEmail(pdfStream, filename, student, dateToReport);
                }

                _logger.LogInformation($" Sending reports to school...");

                // Email all the files to the school
                var attachmentList = new List<Attachment>();

                // Create ZIP file of all attachments, if # of attachments is greater than 4
                if (studentFiles.Count > 4)
                {
                    using var memoryStream = new MemoryStream();
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
                    {
                        foreach (var file in studentFiles)
                        {
                            var zipArchiveEntry = zipArchive.CreateEntry(file.Value);
                            using var streamWriter = new StreamWriter(zipArchiveEntry.Open());
                            var fileData = File.ReadAllBytes(file.Key);
                            streamWriter.BaseStream.Write(fileData, 0, fileData.Length);
                        }
                    }

                    var attachmentStream = new MemoryStream(memoryStream.ToArray());

                    attachmentList.Add(new Attachment(attachmentStream, "Attendance Reports.zip", MediaTypeNames.Application.Zip));
                }
                else
                {
                    foreach (var file in studentFiles)
                    {
                        var fileData = File.ReadAllBytes(file.Key);

                        attachmentList.Add(new Attachment(new MemoryStream(fileData), file.Value, MediaTypeNames.Application.Pdf));
                    }
                }

                // Email the file to the parents
                await SendSchoolEmailAsync(school.Key, attachmentList, dateToReport);

                _logger.LogInformation($" Cleaning up temporary files");

                // Delete all temp files
                foreach (var entry in studentFiles)
                {
                    if (File.Exists(entry.Key))
                        File.Delete(entry.Key);
                }
            }
        }

        private async Task SendParentEmail(MemoryStream pdfStream, string filename, Student student, DateTime dateToReport)
        {
            // Email the file to the parents
            var emailAddresses = await _sentralGateway.GetContactEmailsAsync(student.SentralStudentId);

            var notification = new AttendanceReportEmail
            {
                StudentName = student.DisplayName,
                StartDate = dateToReport,
                EndDate = dateToReport.AddDays(12),
                Recipients = emailAddresses.Select(contact => new EmailBaseClass.Recipient { Name = contact, Email = contact }).ToList(),
                NotificationType = AttendanceReportEmail.NotificationSequence.Student
            };

            notification.Attachments.Add(new Attachment(pdfStream, filename));

            var success = await _emailService.SendAttendanceReport(notification);

            if (success)
            {
                foreach (var email in emailAddresses)
                    _logger.LogInformation($"  Message sent via Email to {email} with attachment: {filename}");
            }
            else
            {
                foreach (var email in emailAddresses)
                    _logger.LogInformation($"  FAILED to send email to {email} with attachment: {filename}");
            }
        }

        private async Task SendSchoolEmailAsync(string schoolCode, List<Attachment> attachmentList, DateTime dateToReport)
        {
            var contacts = await _unitOfWork.SchoolContactRoles.EmailsFromSchoolWithRole(schoolCode, SchoolContactRole.Coordinator);

            var school = await _unitOfWork.Schools.ForEditAsync(schoolCode);
            contacts.Add(school.EmailAddress);

            var notification = new AttendanceReportEmail
            {
                StartDate = dateToReport,
                EndDate = dateToReport.AddDays(12),
                Recipients = contacts.Select(contact => new EmailBaseClass.Recipient { Name = contact, Email = contact }).ToList(),
                NotificationType = AttendanceReportEmail.NotificationSequence.School,
                Attachments = attachmentList
            };

            var success = await _emailService.SendAttendanceReport(notification);

            if (success)
            {
                foreach (var email in contacts)
                    _logger.LogInformation($"  Message sent via Email to {email} with Attendance Reports for {school.Name}");
            }
            else
            {
                foreach (var email in contacts)
                    _logger.LogInformation($"  FAILED to send email to {email} with Attendance Reports for {school.Name}");
            }
        }

        private async Task<ValueTuple<string, string>> GetStudentData(Student student, DateTime startDate)
        {
            var viewModel = new AttendanceReportViewModel
            {
                StudentName = student.DisplayName,
                StartDate = startDate.VerifyStartOfFortnight(),
                ExcludedDates = await _sentralGateway.GetExcludedDatesFromCalendar(startDate.Year.ToString())
            };

            var endDate = viewModel.StartDate.AddDays(12);
            var periodAbsences = await _unitOfWork.Absences
                .ForStudentWithinTimePeriod(student.StudentId, viewModel.StartDate, endDate);
            var convertedAbsences = periodAbsences.Select(AttendanceReportViewModel.AbsenceDto.ConvertFromAbsence).ToList();
            viewModel.Absences = convertedAbsences;

            var ReportableDates = viewModel.StartDate.Range(endDate).ToList();
            foreach (var date in ReportableDates)
            {
                var entry = new AttendanceReportViewModel.DateSessions
                {
                    Date = date,
                    DayNumber = date.GetDayNumber()
                };

                var sessions = await _unitOfWork.OfferingSessions.ForStudentAndDayAtTime(student.StudentId, date.GetDayNumber(), date);
                entry.SessionsByOffering = sessions.OrderBy(s => s.Period.StartTime).GroupBy(s => s.OfferingId).Select(AttendanceReportViewModel.SessionByOffering.ConvertFromSessionGroup).ToList();

                viewModel.DateData.Add(entry);
            }

            var headerString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReportHeader.cshtml", viewModel);
            var htmlString = await _renderService.RenderViewToStringAsync("/Views/Documents/Attendance/AttendanceReport.cshtml", viewModel);

            return new (headerString, htmlString);
        }
    }
}
