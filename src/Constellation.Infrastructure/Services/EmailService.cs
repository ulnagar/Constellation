using Constellation.Application.Common.CQRS.Emails.Commands;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Templates.Views.Emails.Absences;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using Constellation.Infrastructure.Templates.Views.Emails.MissedWork;
using Constellation.Infrastructure.Templates.Views.Emails.RollMarking;
using MediatR;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    public class EmailService : IEmailService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRazorViewToStringRenderer _razorService;
        private readonly IMediator _mediator;

        public EmailService(IUnitOfWork unitOfWork, IRazorViewToStringRenderer razorService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _razorService = razorService;
            _mediator = mediator;
        }

        public async Task SendAttendanceReport(AttendanceReportEmail notification)
        {
            switch (notification.NotificationType)
            {
                case AttendanceReportEmail.NotificationSequence.Student:
                    await SendParentAttendanceReportEmail(notification);
                    break;
                case AttendanceReportEmail.NotificationSequence.School:
                    await SendSchoolAttendanceReportEmail(notification);
                    break;
            }
        }

        private async Task SendParentAttendanceReportEmail(AttendanceReportEmail notification)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new ParentAttendanceReportEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"[Aurora College] Attendance Report {notification.StartDate:dd-MM-yyyy}",
                StudentName = notification.StudentName,
                StartDate = notification.StartDate,
                EndDate = notification.EndDate
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAttendanceReportEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = notification.Attachments
            };

            await _mediator.Send(message);
        }

        private async Task SendSchoolAttendanceReportEmail(AttendanceReportEmail notification)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new SchoolAttendanceReportEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"ATTN: Attendance Coordinator RE: [Aurora College] Attendance Report {notification.StartDate:dd-MM-yyyy}",
                StartDate = notification.StartDate,
                EndDate = notification.EndDate
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = notification.Attachments
            };

            await _mediator.Send(message);
        }

        public async Task<Guid> SendParentWholeAbsenceAlert(List<Absence> absences, List<string> emailAddresses)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new ParentAbsenceNotificationEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
                StudentFirstName = absences.First().Student.FirstName,
                Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Parents/{absences.First().StudentId}",
                Absences = absences.Select(ParentAbsenceNotificationEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in emailAddresses)
            {
                toRecipients.Add(entry, entry);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            var id = await _mediator.Send(message);

            return id;
        }

        public async Task<Guid> SendParentWholeAbsenceDigest(List<Absence> absences, List<string> emailAddresses)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new ParentAbsenceDigestEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
                StudentFirstName = absences.First().Student.FirstName,
                Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Parents/{absences.First().StudentId}",
                Absences = absences.Select(ParentAbsenceDigestEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in emailAddresses)
            {
                toRecipients.Add(entry, entry);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            var id = await _mediator.Send(message);

            return id;
        }

        public async Task<Guid> SendStudentPartialAbsenceExplanationRequest(List<Absence> absences, List<string> emailAddresses)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new StudentAbsenceExplanationRequestEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
                StudentName = absences.First().Student.DisplayName,
                Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Students/{absences.First().StudentId}",
                Absences = absences.Select(StudentAbsenceExplanationRequestEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in emailAddresses)
            {
                toRecipients.Add(entry, entry);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            return await _mediator.Send(message);
        }

        public async Task<Guid> SendCoordinatorPartialAbsenceVerificationRequest(EmailDtos.AbsenceResponseEmail emailDto)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new CoordinatorAbsenceVerificationRequestEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = "Partial Absence Verification Request",
                StudentName = emailDto.PartialAbsences.First().Student.DisplayName,
                SchoolName = emailDto.PartialAbsences.First().Student.School.Name,
                PortalLink = "https://acos.aurora.nsw.edu.au/Portal/Absences/School"
            };

            foreach (var absence in emailDto.PartialAbsences)
            {
                viewModel.ClassList.Add(new CoordinatorAbsenceVerificationRequestEmailViewModel.AbsenceExplanation
                {
                    Date = absence.Date,
                    PeriodName = absence.PeriodName,
                    PeriodTimeframe = absence.PeriodTimeframe,
                    AbsenceTimeframe = absence.AbsenceTimeframe,
                    OfferingName = absence.Offering.Name,
                    Explanation = absence.Responses.First().Explanation
                });
            }

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in emailDto.Recipients)
            {
                toRecipients.Add(entry, entry);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            return await _mediator.Send(message);
        }

        public async Task<Guid> SendCoordinatorWholeAbsenceDigest(List<Absence> absences)
        {
            var coordinators = await _unitOfWork.SchoolContacts.EmailAddressesOfAllInRoleAtSchool(absences.First().Student.SchoolCode, SchoolContactRole.Coordinator);

            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new CoordinatorAbsenceDigestEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = "Absence Explanation Request",
                StudentName = absences.First().Student.DisplayName,
                SchoolName = absences.First().Student.School.Name,
                Link = "https://acos.aurora.nsw.edu.au/Portal/Absences/School",
                Absences = absences.Select(CoordinatorAbsenceDigestEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in coordinators)
                toRecipients.Add(entry, entry);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            return await _mediator.Send(message);
        }

        public async Task SendAdminAbsenceSentralAlert(Student student)
        {
            var viewModel = $"<p>{student.DisplayName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
            };

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "noreply@aurora.nsw.edu.au"),
                Subject = "[Aurora College] Student absence notification",
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendAdminAbsenceContactAlert(Student student)
        {
            var viewModel = $"<p>Parent contact details for {student.DisplayName} cannot be located in Sentral.";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
            };

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "noreply@aurora.nsw.edu.au"),
                Subject = "[Aurora College] Constellation Data Issue Identified",
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendAdminLowCreditAlert(double credit)
        {
            var viewModel = $"<p>The SMS Global account has a low balance of ${credit:c}.</p><p>Please top up the account immediately!</p>";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" },
                { "auroracoll-h.school@det.nsw.edu.au", "auroracoll-h.school@det.nsw.edu.au" }
            };

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "noreply@aurora.nsw.edu.au"),
                Subject = "[Aurora College] SMS Gateway Low Balance Alert",
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }


        public async Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new AbsenceExplanationToSchoolAdminEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = "Absence Explanation Received",
                StudentName = notificationEmail.WholeAbsences.First().Student.DisplayName
            };

            foreach (var absence in notificationEmail.WholeAbsences)
            {
                var reportedBy = "UNKNOWN SOURCE";
                var response = absence.Responses.First();

                if (response.Type == AbsenceResponse.Coordinator)
                    reportedBy = $"Reported by {response.From} (ACC)";
                else if (response.Type == AbsenceResponse.Parent)
                    reportedBy = "Reported by Parent";
                else if (response.Type == AbsenceResponse.Student)
                {
                    var status = (response.VerificationStatus == AbsenceResponse.Verified) ? "verified" : "rejected";

                    reportedBy = $"Reported by Student and <strong>{status}</strong> by {response.Verifier} (ACC)";

                    if (!string.IsNullOrWhiteSpace(response.VerificationComment))
                        reportedBy += $"<br />with comment: {response.VerificationComment}";
                }

                viewModel.Absences.Add(new AbsenceExplanationToSchoolAdminEmailViewModel.AbsenceDto
                {
                    AbsenceDate = absence.Date,
                    PeriodName = $"{absence.PeriodName} ({absence.PeriodTimeframe})",
                    ClassName = absence.Offering.Name,
                    Explanation = absence.Responses.First().Explanation,
                    Source = reportedBy
                });
            }

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/AbsenceExplanationToSchoolAdminEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notificationEmail.Recipients)
            {
                toRecipients.Add(entry, entry);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = $"Absence Explanation Received - {viewModel.StudentName}",
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendNewCoverEmail(EmailDtos.CoverEmail resource)
        {
            var viewModel = new NewCoverEmailViewModel
            {
                ToName = resource.CoveringTeacherName,
                Title = "Class Cover Information",
                SenderName = "Ben Hillsley",
                SenderTitle = "Learning Technologies Support Officer",
                StartDate = resource.StartDate,
                EndDate = resource.EndDate,
                HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
                Preheader = "",
                ClassWithLink = resource.ClassesIncluded
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
            };

            var ccRecipients = new Dictionary<string, string>();

            foreach (var entry in resource.ClassroomTeachers)
                ccRecipients.Add(entry.Key, entry.Value);

            foreach (var entry in resource.SecondaryRecipients)
                if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                    ccRecipients.Add(entry.Key, entry.Value);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "auroracoll-h.school@det.nsw.edu.au"),
                Subject = $"Class Cover Inforation - {resource.StartDate.ToShortDateString()}",
                Body = body,
                Attachments = resource.Attachments
            };

            await _mediator.Send(message);
        }

        public async Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource)
        {
            var viewModel = new UpdatedCoverEmailViewModel
            {
                ToName = resource.CoveringTeacherName,
                Title = "[UPDATED] Class Cover Information",
                SenderName = "Ben Hillsley",
                SenderTitle = "Learning Technologies Support Officer",
                StartDate = resource.StartDate,
                EndDate = resource.EndDate,
                HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
                Preheader = "",
                ClassWithLink = resource.ClassesIncluded
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
            };

            var ccRecipients = new Dictionary<string, string>();

            foreach (var entry in resource.ClassroomTeachers)
                ccRecipients.Add(entry.Key, entry.Value);

            foreach (var entry in resource.SecondaryRecipients)
                if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                    ccRecipients.Add(entry.Key, entry.Value);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "auroracoll-h.school@det.nsw.edu.au"),
                Subject = $"Class Cover Inforation - {resource.StartDate.ToShortDateString()}",
                Body = body,
                Attachments = resource.Attachments
            };

            await _mediator.Send(message);
        }

        public async Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource)
        {
            var viewModel = new CancelledCoverEmailViewModel
            {
                ToName = resource.CoveringTeacherName,
                Title = "[CANCELLED] Class Cover Information",
                SenderName = "Ben Hillsley",
                SenderTitle = "Learning Technologies Support Officer",
                StartDate = resource.StartDate,
                EndDate = resource.EndDate,
                HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
                Preheader = "",
                ClassWithLink = resource.ClassesIncluded
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
            };

            var ccRecipients = new Dictionary<string, string>();

            foreach (var entry in resource.ClassroomTeachers)
                ccRecipients.Add(entry.Key, entry.Value);

            foreach (var entry in resource.SecondaryRecipients)
                if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                    ccRecipients.Add(entry.Key, entry.Value);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "auroracoll-h.school@det.nsw.edu.au"),
                Subject = $"Class Cover Inforation - {resource.StartDate.ToShortDateString()}",
                Body = body,
                Attachments = resource.Attachments
            };

            await _mediator.Send(message);
        }

        public async Task SendLessonMissedEmail(LessonMissedNotificationEmail notification)
        {
            switch (notification.NotificationType)
            {
                case LessonMissedNotificationEmail.NotificationSequence.First:
                    await SendFirstLessonWarningEmail(notification);
                    break;
                case LessonMissedNotificationEmail.NotificationSequence.Second:
                    await SendSecondLessonWarningEmail(notification);
                    break;
                case LessonMissedNotificationEmail.NotificationSequence.Third:
                    await SendThirdLessonWarningEmail(notification);
                    break;
                case LessonMissedNotificationEmail.NotificationSequence.Final:
                    await SendFinalLessonWarningEmail(notification);
                    break;
                case LessonMissedNotificationEmail.NotificationSequence.Alert:
                    await SendLessonAlertEmail(notification);
                    break;
            }
        }

        private async Task SendFirstLessonWarningEmail(LessonMissedNotificationEmail notification)
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = new FirstWarningEmailViewModel
            {
                Preheader = "",
                SenderName = settings.LessonsCoordinatorName,
                SenderTitle = settings.LessonsCoordinatorTitle,
                Title = "[Aurora College] Science Practical Lesson Overdue",
                Link = "https://acos.aurora.nsw.edu.au/Portal/Lessons",
                SchoolName = notification.SchoolName,
                Lessons = notification.Lessons.Select(FirstWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FirstWarningEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", settings.LessonsCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        private async Task SendSecondLessonWarningEmail(LessonMissedNotificationEmail notification)
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = new SecondWarningEmailViewModel
            {
                Preheader = "",
                SenderName = settings.LessonsCoordinatorName,
                SenderTitle = settings.LessonsCoordinatorTitle,
                Title = "[Aurora College] Science Practical Lesson Overdue",
                Link = "https://acos.aurora.nsw.edu.au/Portal/Lessons",
                SchoolName = notification.SchoolName,
                Lessons = notification.Lessons.Select(SecondWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", settings.LessonsCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        private async Task SendThirdLessonWarningEmail(LessonMissedNotificationEmail notification)
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = new SecondWarningEmailViewModel
            {
                Preheader = "",
                SenderName = settings.LessonsCoordinatorName,
                SenderTitle = settings.LessonsCoordinatorTitle,
                Title = "[Aurora College] Science Practical Lesson Overdue",
                Link = "https://acos.aurora.nsw.edu.au/Portal/Lessons",
                SchoolName = notification.SchoolName,
                Lessons = notification.Lessons.Select(SecondWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", settings.LessonsCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        private async Task SendFinalLessonWarningEmail(LessonMissedNotificationEmail notification)
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = new FinalWarningEmailViewModel
            {
                Preheader = "",
                SenderName = settings.LessonsCoordinatorName,
                SenderTitle = settings.LessonsCoordinatorTitle,
                Title = "[Aurora College] Science Practical Lesson Overdue",
                Link = "https://acos.aurora.nsw.edu.au/Portal/Lessons",
                SchoolName = notification.SchoolName,
                Lessons = notification.Lessons.Select(FinalWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FinalWarningEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", settings.LessonsCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        private async Task SendLessonAlertEmail(LessonMissedNotificationEmail notification)
        {
            var settings = await _unitOfWork.Settings.Get();

            var viewModel = new CoordinatorNotificationEmailViewModel
            {
                Preheader = "",
                SenderName = settings.LessonsCoordinatorName,
                SenderTitle = settings.LessonsCoordinatorTitle,
                Title = "[Aurora College] Science Practical Lesson Overdue",
                SchoolName = notification.SchoolName,
                Lessons = notification.Lessons.Select(CoordinatorNotificationEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/CoordinatorNotificationEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", settings.LessonsCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendServiceLogEmail(ServiceLogEmail notification)
        {
            var viewModel = $"The following messages were logged by {notification.Source} when it ran today.<br>";
            foreach (var line in notification.Log)
                viewModel += line + "<br>";

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>
            {
                { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
            };

            foreach (var entry in notification.Recipients)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", "noreply@aurora.nsw.edu.au"),
                Subject = $"[Aurora College] Service Log Output - {notification.Source}",
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendTeacherClassworkNotificationRequest(ClassworkNotificationTeacherEmail notification)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new TeacherMissedWorkNotificationEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = "[Aurora College] Missed Classwork Notifications",
                OfferingName = notification.OfferingName,
                AbsenceDate = notification.AbsenceDate,
                StudentList = notification.Students,
                Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Teachers/{notification.NotificationId}",
                IsCovered = notification.IsCovered
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/TeacherMissedWorkNotificationEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in notification.Teachers)
                toRecipients.Add(entry.Name, entry.Email);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new("Aurora College", absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendStudentClassworkNotification(Absence absence, ClassworkNotification notification, List<string> parentEmails)
        {
            var viewModel = new StudentMissedWorkNotificationEmailViewModel
            {
                Preheader = "",
                SenderName = notification.CompletedBy.DisplayName,
                SenderTitle = "Teacher",
                Title = $"[Aurora College] Missed Classwork Notification - {notification.Offering.Name} - {notification.AbsenceDate.ToShortDateString()}",
                OfferingName = notification.Offering.Name,
                AbsenceDate = notification.AbsenceDate,
                CourseName = notification.Offering.Course.Name,
                StudentName = absence.Student.FirstName,
                WorkDescription = notification.Description
            };

            var toRecipients = new Dictionary<string, string>();

            if (!absence.Explained)
            {
                await SendParentClassworkNotification(absence, notification, parentEmails);
            }
            else
            {
                foreach (var entry in parentEmails)
                    toRecipients.Add(entry, entry);
            }

            toRecipients.Add(absence.Student.DisplayName, absence.Student.EmailAddress);

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/StudentMissedWorkNotificationEmail.cshtml", viewModel);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new(notification.CompletedBy.DisplayName, notification.CompletedBy.EmailAddress),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        private async Task SendParentClassworkNotification(Absence absence, ClassworkNotification notification, List<string> parentEmails)
        {
            var viewModel = new ParentMissedWorkNotificationEmailViewModel
            {
                Preheader = "",
                SenderName = notification.CompletedBy.DisplayName,
                SenderTitle = "Teacher",
                Title = $"[Aurora College] Missed Classwork Notification - {notification.Offering.Name} - {notification.AbsenceDate.ToShortDateString()}",
                OfferingName = notification.Offering.Name,
                AbsenceDate = notification.AbsenceDate,
                CourseName = notification.Offering.Course.Name,
                StudentName = absence.Student.FirstName,
                WorkDescription = notification.Description,
                Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Parents/{absence.StudentId}"
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/ParentMissedWorkNotificationEmail.cshtml", viewModel);
            
            var toRecipients = new Dictionary<string, string>();
            foreach (var entry in parentEmails)
                toRecipients.Add(entry, entry);

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new(notification.CompletedBy.DisplayName, notification.CompletedBy.EmailAddress),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }

        public async Task SendDailyRollMarkingReport(List<RollMarkReportDto> orderedEntries, bool completeReport)
        {
            var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

            var viewModel = new DailyReportEmailViewModel
            {
                Preheader = "",
                SenderName = absenceSettings.AbsenceCoordinatorName,
                SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
                Title = $"[Aurora College] Roll Marking Report - {orderedEntries.First().Date.ToShortDateString()}",
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            if (completeReport)
            {
                // Send the Absence Coordinator
                toRecipients.Add(absenceSettings.AbsenceCoordinatorName, absenceSettings.AbsenceCoordinatorEmail);
            }
            else
            {
                // Send to the Classroom Teacher
                toRecipients.Add(orderedEntries.First().EmailSentTo, orderedEntries.First().EmailSentTo);
            }

            var message = new AddEmailToQueue
            {
                ToAddresses = toRecipients,
                CcAddresses = null,
                BccAddresses = null,
                FromAddress = new(absenceSettings.AbsenceCoordinatorName, absenceSettings.AbsenceCoordinatorEmail),
                Subject = viewModel.Title,
                Body = body,
                Attachments = null
            };

            await _mediator.Send(message);
        }
    }
}
