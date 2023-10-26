namespace Constellation.Infrastructure.ExternalServices.Email;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Application.Interfaces.Configuration;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.Assignments;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects;
using Constellation.Core.ValueObjects;
using Constellation.Infrastructure.Templates.Views.Emails.Absences;
using Constellation.Infrastructure.Templates.Views.Emails.Auth;
using Constellation.Infrastructure.Templates.Views.Emails.Awards;
using Constellation.Infrastructure.Templates.Views.Emails.Covers;
using Constellation.Infrastructure.Templates.Views.Emails.Lessons;
using Constellation.Infrastructure.Templates.Views.Emails.MandatoryTraining;
using Constellation.Infrastructure.Templates.Views.Emails.Reports;
using Constellation.Infrastructure.Templates.Views.Emails.RollMarking;
using Core.Shared;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Threading;
using Templates.Views.Emails.Assignments;

public class Service : IEmailService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly AppConfiguration _configuration;

    public Service(
        IUnitOfWork unitOfWork,
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IRazorViewToStringRenderer razorService,
        IOptions<AppConfiguration> configuration)
    {
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
        _calendarService = calendarService;
        _razorService = razorService;
        _configuration = configuration.Value;
    }

    public async Task SendAcademicReportToNonResidentialParent(
        List<EmailRecipient> recipients, 
        Name studentName, 
        string ReportingPeriod, 
        string Year, 
        FileDto file,
        CancellationToken cancellationToken = default)
    {
        List<Attachment> attachments = new();
        var stream = new MemoryStream(file.FileData);
        attachments.Add(new Attachment(stream, file.FileName, file.FileType));

        foreach (var parent in recipients)
        {
            var viewModel = new AcademicReportEmailViewModel
            {
                Preheader = "",
                SenderName = "Chris Robertson",
                SenderTitle = "Principal",
                Title = $"[Aurora College] Academic Report Published",
                ParentName = parent.Name,
                StudentName = studentName,
                ReportingPeriod = ReportingPeriod,
                Year = Year
            };

            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Reports/AcademicReportEmail.cshtml", viewModel);

            var toRecipients = new Dictionary<string, string>();
            if (!toRecipients.Any(recipient => recipient.Value == parent.Email))
                toRecipients.Add(parent.Name, parent.Email);

            await _emailSender.Send(toRecipients, null, viewModel.Title, body, attachments, cancellationToken);
        }
    }

    public async Task<bool> SendParentAttendanceReportEmail(
        string studentName, 
        DateOnly startDate, 
        DateOnly endDate, 
        List<EmailRecipient> recipients, 
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new ParentAttendanceReportEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StudentName = studentName,
            StartDate = startDate,
            EndDate = endDate
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAttendanceReportEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
            return true;
        else
            return false;
    }

    public async Task<bool> SendSchoolAttendanceReportEmail(
        DateOnly startDate,
        DateOnly endDate,
        List<EmailRecipient> recipients,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new SchoolAttendanceReportEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"ATTN: Attendance Coordinator RE: [Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StartDate = startDate,
            EndDate = endDate
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
            return true;
        else
            return false;
    }

    public async Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(
        List<AbsenceEntry> absences, 
        Student student,
        List<EmailRecipient> emailAddresses,
        CancellationToken cancellationToken = default)
    {
        ParentAbsenceNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = student.FirstName,
            Absences = absences
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in emailAddresses)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(
        List<AbsenceEntry> absences, 
        Student student,
        List<EmailRecipient> emailAddresses,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new ParentAbsenceDigestEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = student.FirstName,
            Absences = absences
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in emailAddresses)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(
        List<AbsenceEntry> absences, 
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new StudentAbsenceExplanationRequestEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.DisplayName,
            Link = $"https://acos.aurora.nsw.edu.au/Portal/Absences/Students/{student.StudentId}",
            Absences = absences
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml", viewModel);

        var message = await _emailSender.Send(recipients, null, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(
        List<AbsenceExplanation> absences,
        Student student, 
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new CoordinatorAbsenceVerificationRequestEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Partial Absence Verification Request",
            StudentName = student.DisplayName,
            SchoolName = student.School.Name,
            ClassList = absences
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(
        List<AbsenceEntry> absences, 
        Student student,
        School school,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Count == 0)
            return null;

        var viewModel = new CoordinatorAbsenceDigestEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Request",
            StudentName = student.DisplayName,
            SchoolName = school.Name,
            Absences = absences
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        var message = await _emailSender.Send(toRecipients, null, viewModel.Title, body, cancellationToken);

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

    public async Task SendMissedWorkEmail(
        Student student, 
        string subjectName,
        string className,
        DateOnly absenceDate,
        List<EmailRecipient> recipients, 
        CancellationToken cancellationToken = default)
    {
        var viewModel = new MissedWorkEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "[Aurora College] Missed Classwork Notification",
            StudentName = student.DisplayName,
            Subject = subjectName,
            ClassName = className,
            AbsenceDate = absenceDate
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/MissedWorkEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, cancellationToken);
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

    public async Task SendParentContactChangeReportEmail(
        MemoryStream report,
        CancellationToken cancellationToken = default)
    {
        var viewModel = $"<p>Parent Contact Change Report for {DateTime.Today.ToLongDateString()} is attached.</p>";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" },
            { "Aurora College", "auroracoll-h.school@det.nsw.edu.au" }
        };

        var attachments = new List<Attachment>
        {
            new Attachment(report, "Change Report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        };

        await _emailSender.Send(toRecipients, null, $"[Aurora College] Parent Contact Change Report - {DateTime.Today.ToLongDateString()}", body, attachments, cancellationToken);
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

    public async Task SendAssignmentUploadFailedNotification(
        string assignmentName,
        AssignmentId assignmentId,
        string studentName,
        AssignmentSubmissionId submissionId,
        CancellationToken cancellationToken = default)
    {
        var viewModel = $@"<p>Failed to upload an assignment submission to Canvas:</p>
            <dl>
                <dt>Assignment:</dt>
                <dd>{assignmentName} ({assignmentId.Value})</dd>
                <dt>Submission:</dt>
                <dd>From {studentName} ({submissionId.Value})</dd>
            </dl>";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "auroracollegeitsupport@det.nsw.edu.au", "auroracollegeitsupport@det.nsw.edu.au" }
        };

        await _emailSender.Send(
            toRecipients,
            "noreply@aurora.nsw.edu.au",
            "[Aurora College] Canvas Assignment Upload Failure",
            body,
            cancellationToken);
    }


    public async Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        var viewModel = new AbsenceExplanationToSchoolAdminEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
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
                Type = absence.AbsenceType.Value,
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

    public async Task SendNewCoverEmail(
        ClassCover cover,
        Offering offering,
        EmailRecipient coveringTeacher,
        List<EmailRecipient> primaryRecipients,
        List<EmailRecipient> secondaryRecipients,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        var singleDayCover = cover.StartDate == cover.EndDate;

        // Send
        var viewModel = new NewCoverEmailViewModel
        {
            ToName = coveringTeacher.Name,
            Title = $"Aurora Class Cover - {offering.Name}",
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
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            var uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            var summary = $"Aurora College Cover - {offering.Name}";
            var location = $"Class Team ({teamLink})";
            var description = body;

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            var appointmentStart = cover.StartDate.ToDateTime(startTime);
            var appointmentEnd = cover.EndDate.ToDateTime(endTime);

            var icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, description, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, cancellationToken);
        }

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

    public async Task SendUpdatedCoverEmail(
        ClassCover cover,
        Offering offering,
        EmailRecipient coveringTeacher,
        List<EmailRecipient> primaryRecipients,
        List<EmailRecipient> secondaryRecipients,
        DateOnly originalStartDate,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        var singleDayCover = cover.StartDate == cover.EndDate;

        var viewModel = new UpdatedCoverEmailViewModel
        {
            ToName = coveringTeacher.Name,
            Title = $"[UPDATED] Aurora Class Cover - {offering.Name}",
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
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            var uid = $"{cover.Id}-{cover.OfferingId}-{originalStartDate:yyyyMMdd}";
            var summary = $"[UPDATED] Aurora Class Cover - {offering.Name}";
            var location = $"Class Team ({teamLink})";
            var description = body;

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            var appointmentStart = cover.StartDate.ToDateTime(startTime);
            var appointmentEnd = cover.EndDate.ToDateTime(endTime);

            var icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, description, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, cancellationToken);
        }
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
        Offering offering,
        EmailRecipient coveringTeacher,
        List<EmailRecipient> primaryRecipients,
        List<EmailRecipient> secondaryRecipients,
        TimeOnly startTime,
        TimeOnly endTime,
        string teamLink,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        // Determine whether email or invite
        var singleDayCover = cover.StartDate == cover.EndDate;

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

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients.ToDictionary(k => k.Name, k => k.Email), secondaryRecipients.ToDictionary(k => k.Name, k => k.Email), "auroracoll-h.school@det.nsw.edu.au", viewModel.Title, body, attachments, cancellationToken);
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
            Link = "https://acos.aurora.nsw.edu.au/schools",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
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
            Link = "https://acos.aurora.nsw.edu.au/schools",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
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
            Link = "https://acos.aurora.nsw.edu.au/schools",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
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
            Link = "https://acos.aurora.nsw.edu.au/schools",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
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
            Lessons = notification.Lessons
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/CoordinatorNotificationEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>();
        foreach (var entry in notification.Recipients)
            if (!toRecipients.Any(recipient => recipient.Value == entry.Email))
                toRecipients.Add(entry.Name, entry.Email);

        await _emailSender.Send(toRecipients, settings.LessonsCoordinatorEmail, viewModel.Title, body);
    }

    public async Task SendStudentLessonCompletedEmail(
        Student student,
        string lessonName,
        string courseName,
        CancellationToken cancellationToken)
    {
        var viewModel = new StudentMarkedPresentEmailViewModel
        {
            Title = $"Congratulations on finishing your Science Prac!",
            SenderName = "Silvia Rudmann",
            SenderTitle = "R/Head Teacher Science and Agriculture",
            Preheader = "",
            StudentName = student.DisplayName,
            LessonTitle = lessonName,
            Subject = courseName
        };

        var toRecipients = new Dictionary<string, string>
            {
                { student.DisplayName, student.EmailAddress }
            };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/StudentMarkedPresentEmail.cshtml", viewModel);

        await _emailSender.Send(toRecipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body, cancellationToken);
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

    public async Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, Dictionary<string, string> recipients)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new DailyReportEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}",
            RollEntries = entries
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, null, viewModel.Title, body);
    }

    public async Task SendNoRollMarkingReport(DateOnly reportDate, Dictionary<string, string> recipients)
    {
        var absenceSettings = await _unitOfWork.Settings.GetAbsenceAppSettings();

        var viewModel = new DailyReportEmailViewModel
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}"
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/NoReportEmail.cshtml", viewModel);

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

    public async Task SendTrainingExpiryWarningEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients)
    {
        var viewModel = new TrainingExpiringSoonWarningEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Name}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendTrainingExpiryAlertEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients)
    {
        var viewModel = new TrainingExpiringSoonAlertEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Name}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendTrainingExpiredEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients)
    {
        var viewModel = new TrainingExpiredAlertEmailViewModel
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Mandatory Training Expiry Warning - {recipients.First().Name}",
            Courses = courses
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/MandatoryTraining/TrainingExpiryWarningEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body);
    }

    public async Task SendMasterFileConsistencyReportEmail(MemoryStream report, string emailAddress, CancellationToken cancellationToken = default)
    {
        var viewModel = $"<p>MasterFile Consistency Report generated {DateTime.Today.ToLongDateString()} is attached.</p>";

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        var toRecipients = new Dictionary<string, string>
        {
            { "", emailAddress }
        };

        var attachments = new List<Attachment>
        {
            new Attachment(report, "Consistency Report.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        };

        await _emailSender.Send(toRecipients, null, $"[Aurora College] MasterFile Consistency Report - {DateTime.Today.ToLongDateString()}", body, attachments, cancellationToken);
    }

    public async Task SendAwardCertificateParentEmail(
        List<EmailRecipient> recipients,
        Attachment certificate, 
        StudentAward award,
        Student? student,
        Staff? teacher,
        CancellationToken cancellationToken = default)
    {
        var viewModel = new NewAwardCertificateEmailViewModel
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Student Award for {student.DisplayName}",
            AwardType = award.Type,
            AwardedOn = award.AwardedOn,
            AwardReason = award.Reason,
            StudentName = student?.DisplayName,
            TeacherName = teacher?.DisplayName
        };

        var body = await _razorService.RenderViewToStringAsync("/Views/Emails/Awards/NewAwardCertificateEmail.cshtml", viewModel);
        
        foreach (var recipient in recipients)
        {
            await _emailSender.Send(new List<EmailRecipient> { recipient }, "noreply@aurora.nsw.edu.au", viewModel.Title, body, new List<Attachment> { certificate }, cancellationToken);
        }
    }

    public async Task<bool> SendAssignmentUploadReceipt(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        Course course,
        Student student,
        CancellationToken cancellationToken = default)
    {
        AssignmentSubmissionUploadReceiptEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Student Assignment Upload Receipt",
            AssignmentName = assignment.Name,
            CourseName = course.Name,
            StudentName = student.GetName()?.DisplayName,
            SubmittedOn = DateOnly.FromDateTime(submission.SubmittedOn)
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Assignments/AssignmentSubmissionUploadReceiptEmail.cshtml", viewModel);

        List<EmailRecipient> recipients = new();

        Result<EmailRecipient> recipient = EmailRecipient.Create(submission.SubmittedBy, submission.SubmittedBy);

        if (recipient.IsFailure)
        {
            return false;
        }

        recipients.Add(recipient.Value);

        await _emailSender.Send(recipients, "noreply@aurora.nsw.edu.au", viewModel.Title, body, cancellationToken);

        return true;
    }
}
