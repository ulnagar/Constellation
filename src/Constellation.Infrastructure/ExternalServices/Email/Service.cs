using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.Covers;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.DependencyInjection;
using Constellation.Infrastructure.Templates.Views.Emails.Absences;
using Constellation.Infrastructure.Templates.Views.Emails.Auth;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;
using Constellation.Infrastructure.Templates.Views.Emails.MissedWork;
using Constellation.Infrastructure.Templates.Views.Emails.RollMarking;
using System.Net.Mail;

namespace Constellation.Infrastructure.ExternalServices.Email;

public class Service : IEmailService, IScopedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IRazorViewToStringRenderer _razorService;

    public Service(
        IUnitOfWork unitOfWork,
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IRazorViewToStringRenderer razorService)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _calendarService = calendarService;
        _razorService = razorService;
    }

    public async Task<bool> SendAttendanceReport(AttendanceReportEmail notification)
    {
        return notification.NotificationType switch
        {
            AttendanceReportEmail.NotificationSequence.Student => await SendParentAttendanceReportEmail(notification),
            AttendanceReportEmail.NotificationSequence.School => await SendSchoolAttendanceReportEmail(notification),
            _ => false,
        };
    }

    private async Task<bool> SendParentAttendanceReportEmail(AttendanceReportEmail notification)
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, null, null, viewModel.Title, body, notification.Attachments);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
            return true;
        else
            return false;
    }

    private async Task<bool> SendSchoolAttendanceReportEmail(AttendanceReportEmail notification)
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, null, null, viewModel.Title, body, notification.Attachments);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
            return true;
        else
            return false;
    }

    public async Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<Absence> absences, List<string> emailAddresses)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new ParentAbsenceNotificationEmailViewModel
        {
            Preheader = "",
            SenderName = absenceSettings.AbsenceCoordinatorName,
            SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = absences.First().Student.FirstName,
            Absences = absences.Select(ParentAbsenceNotificationEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in emailAddresses)
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new EmailDtos.SentEmail
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }
        else
            return null;
    }

    public async Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(List<Absence> absences, List<string> emailAddresses)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new ParentAbsenceDigestEmailViewModel
        {
            Preheader = "",
            SenderName = absenceSettings.AbsenceCoordinatorName,
            SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = absences.First().Student.FirstName,
            Absences = absences.Select(ParentAbsenceDigestEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in emailAddresses)
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new EmailDtos.SentEmail
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }
        else
            return null;
    }

    public async Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<Absence> absences, List<string> emailAddresses)
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
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new EmailDtos.SentEmail
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }
        else
            return null;
    }

    public async Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(EmailDtos.AbsenceResponseEmail emailDto)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new CoordinatorAbsenceVerificationRequestEmailViewModel
        {
            Preheader = "",
            SenderName = absenceSettings.AbsenceCoordinatorName,
            SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
            Title = "Partial Absence Verification Request",
            StudentName = emailDto.PartialAbsences.First().Student.DisplayName,
            SchoolName = emailDto.PartialAbsences.First().Student.School.Name
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
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new EmailDtos.SentEmail
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }
        else
            return null;
    }

    public async Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(List<Absence> absences)
    {
        var coordinators = await _unitOfWork.SchoolContacts.EmailAddressesOfAllInRoleAtSchool(absences.First().StudentId, SchoolContactRole.Coordinator);

        coordinators = coordinators.Distinct().ToList();

        if (coordinators == null || coordinators.Count == 0)
            return null;

        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new CoordinatorAbsenceDigestEmailViewModel
        {
            Preheader = "",
            SenderName = absenceSettings.AbsenceCoordinatorName,
            SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Request",
            StudentName = absences.First().Student.DisplayName,
            SchoolName = absences.First().Student.School.Name,
            Absences = absences.Select(CoordinatorAbsenceDigestEmailViewModel.AbsenceEntry.ConvertFromAbsence).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in coordinators)
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new EmailDtos.SentEmail
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }
        else
            return null;

    }

    public async Task SendAdminAbsenceSentralAlert(string studentName)
    {
        var viewModel = $"<p>{studentName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
        };

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "[Aurora College] Student absence notification", body);
    }

    public async Task SendAdminAbsenceContactAlert(string studentName)
    {
        var viewModel = $"<p>Parent contact details for {studentName} cannot be located in Sentral.";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
        };

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "[Aurora College] Constellation Data Issue Identified", body);
    }

    public async Task SendAdminClassworkNotificationContactAlert(Student student, Staff teacher, ClassworkNotification notification)
    {
        var viewModel = $"<p>The student {student.DisplayName} did not receive notification of the classwork required to catch up on their absence ({notification.Id}) as there are no parent contact details available in Sentral.</p>";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" },
            { teacher.DisplayName, teacher.EmailAddress }
        };

        await _emailSender.Send(toRecipients, null, "[Aurora College] Constellation Data Issue Identified", body);
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

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", "[Aurora College] SMS Gateway Low Balance Alert", body);
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
            StudentName = notificationEmail.StudentName
        };

        foreach (var absence in notificationEmail.WholeAbsences)
        {
            viewModel.Absences.Add(new AbsenceExplanationToSchoolAdminEmailViewModel.AbsenceDto
            {
                AbsenceDate = absence.AbsenceDate,
                PeriodName = absence.PeriodName,
                ClassName = absence.ClassName,
                Explanation = absence.Explanation,
                Source = absence.ReportedBy,
                Type = absence.AbsenceType,
                AbsenceTime = absence.AbsenceTimeframe
            });
        }

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/AbsenceExplanationToSchoolAdminEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notificationEmail.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        await _emailSender.Send(toRecipients, null, $"Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task SendNewCoverEmail(EmailDtos.CoverEmail resource)
    {
        var viewModel = new NewCoverEmailViewModel
        {
            ToName = resource.CoveringTeacherName,
            Title = "Class Cover Information",
            SenderName = "Cathy Crouch",
            SenderTitle = "Casual Coordinator",
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (var entry in resource.SecondaryRecipients)
            if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, "auroracoll-h.school@det.nsw.edu.au", $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
    }

    public async Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource)
    {
        var viewModel = new UpdatedCoverEmailViewModel
        {
            ToName = resource.CoveringTeacherName,
            Title = "[UPDATED] Class Cover Information",
            SenderName = "Cathy Crouch",
            SenderTitle = "Casual Coordinator",
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (var entry in resource.SecondaryRecipients)
            if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, "auroracoll-h.school@det.nsw.edu.au", $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
    }

    public async Task SendCancelledCoverEmail(
        ClassCover cover,
        CourseOffering offering,
        EmailAddress coveringTeacher,
        List<EmailAddress> primaryRecipients,
        List<EmailAddress> secondaryRecipients,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        CancellationToken cancellationToken)
    {
        // Determine whether email or invite
        var singleDayCover = cover.StartDate == cover.EndDate;

        // Prepare attachments
        var attachments = new List<Attachment>();

        // Send
        var viewModel = new CancelledCoverEmailViewModel
        {
            ToName = coveringTeacher.Name,
            Title = $"Cancelled Aurora Class Cover - {offering.Name}",
            SenderName = "Cathy Crouch",
            SenderTitle = "Casual Coordinator",
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            var uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            var summary = $"Aurora College Cover - {offering.Name}";
            var location = $"Class Team ({teamLink}";
            var description = body;

            var appointmentStart = cover.StartDate.ToDateTime(startTime);
            var appointmentEnd = cover.EndDate.ToDateTime(endTime);
            var icsData = _calendarService.CancelInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, description, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData);
        }
        else
        {
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments);
        }
    }

    public async Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource)
    {
        var viewModel = new CancelledCoverEmailViewModel
        {
            ToName = resource.CoveringTeacherName,
            Title = "[CANCELLED] Class Cover Information",
            SenderName = "Cathy Crouch",
            SenderTitle = "Casual Coordinator",
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (var entry in resource.SecondaryRecipients)
            if (!ccRecipients.Any(recipient => recipient.Value == entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, "auroracoll-h.school@det.nsw.edu.au", $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
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
            Link = "https://web1.auroracoll-h.schools.nsw.edu.au/Portal/School",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons.Select(FirstWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FirstWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
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
            Link = "https://web1.auroracoll-h.schools.nsw.edu.au/Portal/School",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons.Select(SecondWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
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
            Link = "https://web1.auroracoll-h.schools.nsw.edu.au/Portal/School",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons.Select(SecondWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
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
            Link = "https://web1.auroracoll-h.schools.nsw.edu.au/Portal/School",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons.Select(FinalWarningEmailViewModel.LessonEntry.ConvertFromLessonItem).ToList()
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FinalWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
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
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", $"[Aurora College] Service Log Output - {notification.Source}", body);
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
            Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Teachers/Update/{notification.NotificationId}",
            IsCovered = notification.IsCovered
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/TeacherMissedWorkNotificationEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Teachers)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, null, viewModel.Title, body);
    }

    public async Task SendTeacherClassworkNotificationCopy(Absence absence, ClassworkNotification notification, Staff teacher)
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

        var toRecipients = new Dictionary<string, string>
        {
            { teacher.DisplayName, teacher.EmailAddress }
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/StudentMissedWorkNotificationEmail.cshtml", viewModel);

        await _emailSender.Send(toRecipients, notification.CompletedBy.EmailAddress, viewModel.Title, body);
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

        if (parentEmails != null)
        {
            if (!absence.Explained)
            {
                await SendParentClassworkNotification(absence, notification, parentEmails);
            }
            else
            {
                foreach (var entry in parentEmails)
                    toRecipients.Add(entry, entry);
            }
        }

        toRecipients.Add(absence.Student.DisplayName, absence.Student.EmailAddress);

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MissedWork/StudentMissedWorkNotificationEmail.cshtml", viewModel);

        await _emailSender.Send(toRecipients, notification.CompletedBy.EmailAddress, viewModel.Title, body);
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
            if (!toRecipients.Any(recipient => recipient.Value == entry))
                toRecipients.Add(entry, entry);

        await _emailSender.Send(toRecipients, notification.CompletedBy.EmailAddress, viewModel.Title, body);
    }

    public async Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, Dictionary<string, string> recipients)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new DailyReportEmailViewModel
        {
            Preheader = "",
            SenderName = absenceSettings.AbsenceCoordinatorName,
            SenderTitle = absenceSettings.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}",
            RollEntries = entries
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, null, viewModel.Title, body);
    }

    public async Task SendMagicLinkLoginEmail(MagicLinkEmail notification)
    {
        var settings = await _unitOfWork.Settings.Get();

        var viewModel = new MagicLinkLoginEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Portal Login Link",
            ToName = notification.Name,
            Link = notification.Link
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Auth/MagicLinkLoginEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendTrainingExpiryWarningEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients)
    {
        var viewModel = new TrainingExpiringSoonWarningEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Key}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var recipient in recipients)
        {
            toRecipients.Add(recipient.Key, recipient.Value);
        }

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendTrainingExpiryAlertEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients)
    {
        var viewModel = new TrainingExpiringSoonAlertEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Key}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var recipient in recipients)
        {
            toRecipients.Add(recipient.Key, recipient.Value);
        }

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendTrainingExpiredEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients)
    {
        var viewModel = new TrainingExpiredAlertEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Key}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var recipient in recipients)
        {
            toRecipients.Add(recipient.Key, recipient.Value);
        }

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }
}
