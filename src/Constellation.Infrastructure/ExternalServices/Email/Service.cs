namespace Constellation.Infrastructure.ExternalServices.Email;

using Application.Absences.ConvertAbsenceToAbsenceEntry;
using Application.Absences.ConvertResponseToAbsenceExplanation;
using Application.DTOs;
using Application.DTOs.EmailRequests;
using Application.Helpers;
using Application.Interfaces.Configuration;
using Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Core.Abstractions.Clock;
using Core.Extensions;
using Core.Models;
using Core.Models.Assignments;
using Core.Models.Assignments.Identifiers;
using Core.Models.Attendance;
using Core.Models.Awards;
using Core.Models.Covers;
using Core.Models.Offerings;
using Core.Models.SchoolContacts;
using Core.Models.Students;
using Core.Models.Subjects;
using Core.Models.ThirdPartyConsent;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Net.Mail;
using System.Threading;
using Templates.Views.Emails.Absences;
using Templates.Views.Emails.Assignments;
using Templates.Views.Emails.AttendancePlans;
using Templates.Views.Emails.Auth;
using Templates.Views.Emails.Awards;
using Templates.Views.Emails.Contacts;
using Templates.Views.Emails.Covers;
using Templates.Views.Emails.Lessons;
using Templates.Views.Emails.Reports;
using Templates.Views.Emails.RollMarking;
using Templates.Views.Emails.ThirdParty;
using Templates.Views.Emails.WorkFlow;
using Action = Core.Models.WorkFlow.Action;

public sealed class Service : IEmailService
{
    private readonly IEmailGateway _emailSender;
    private readonly ICalendarService _calendarService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IRazorViewToStringRenderer _razorService;
    private readonly ILogger _logger;
    private readonly AppConfiguration _configuration;

    public Service(
        IEmailGateway emailSender,
        ICalendarService calendarService,
        IDateTimeProvider dateTime,
        IRazorViewToStringRenderer razorService,
        IOptions<AppConfiguration> configuration,
        ILogger logger)
    {
        _emailSender = emailSender;
        _calendarService = calendarService;
        _dateTime = dateTime;
        _razorService = razorService;
        _logger = logger.ForContext<IEmailService>();
        _configuration = configuration.Value;
    }

    public async Task SendAttendancePlanToAdmin(
        List<EmailRecipient> recipients,
        AttendancePlan plan,
        CancellationToken cancellationToken = default)
    {
        AttendancePlanDetailsOfUnavailabilityEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Attendance Plan Details",
            Student = plan.Student.DisplayName,
            Grade = plan.Grade.AsName(),
            School = plan.School
        };

        List<AttendancePlanDetailsOfUnavailabilityEmailViewModel.Unavailability> unavailabilities = new();

        foreach (var period in plan.Periods)
        {
            if (period.EntryTime != period.StartTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start= period.StartTime,
                    End = period.EntryTime
                });
            }

            if (period.ExitTime != period.EndTime)
            {
                unavailabilities.Add(new()
                {
                    Week = period.Week,
                    Day = period.Day,
                    Start = period.ExitTime,
                    End = period.EndTime
                });
            }
        }

        viewModel.Unavailabilities = unavailabilities;
        
        string body = await _razorService.RenderViewToStringAsync(AttendancePlanDetailsOfUnavailabilityEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(
            toRecipients: recipients, 
            fromRecipient: EmailRecipient.AuroraCollege, 
            subject: viewModel.Title, 
            body: body, 
            cancellationToken: cancellationToken);
    }


    public async Task SendAcademicReportToNonResidentialParent(
        List<EmailRecipient> recipients, 
        Name studentName, 
        string reportingPeriod, 
        string year, 
        FileDto file,
        CancellationToken cancellationToken = default)
    {
        List<Attachment> attachments = new();
        MemoryStream stream = new(file.FileData);
        attachments.Add(new Attachment(stream, file.FileName, file.FileType));

        foreach (EmailRecipient parent in recipients)
        {
            AcademicReportEmailViewModel viewModel = new()
            {
                Preheader = "",
                SenderName = "Chris Robertson",
                SenderTitle = "Principal",
                Title = $"[Aurora College] Academic Report Published",
                ParentName = parent.Name,
                StudentName = studentName,
                ReportingPeriod = reportingPeriod,
                Year = year
            };

            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Reports/AcademicReportEmail.cshtml", viewModel);

            await _emailSender.Send(recipients, null, viewModel.Title, body, attachments, cancellationToken);
        }
    }

    public async Task SendConsentTransactionReceiptToParent(
        List<EmailRecipient> recipients,
        string studentName,
        DateOnly submittedOn,
        Attachment attachment,
        CancellationToken cancellationToken = default)
    {
        TransactionReceiptParentEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Title = $"[Aurora College] Third-party consent receipt - {studentName} {submittedOn:dd-MM-yyyy}",
            StudentName = studentName,
            SubmittedOn = submittedOn
        };

        string body = await _razorService.RenderViewToStringAsync(TransactionReceiptParentEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, null, null, null, viewModel.Title, body, [ attachment ], cancellationToken);
    }

    public async Task SendConsentRefusedNotification(
        List<EmailRecipient> recipients,
        string studentName,
        DateOnly submittedOn,
        List<Transaction.ConsentResponse> responses,
        CancellationToken cancellationToken = default)
    {
        ConsentRefusedNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = string.Empty,
            SenderTitle = string.Empty,
            Student = studentName,
            SubmittedOn = submittedOn,
            RefusedConsents = responses.Select(entry => entry.ApplicationName).ToList(),
            Title = $"[Aurora College] Third-party consent refused - {studentName} {submittedOn:dd-MM-yyyy}"
        };

        string body = await _razorService.RenderViewToStringAsync(ConsentRefusedNotificationEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);
    }

    public async Task<bool> SendParentAttendanceReportEmail(
        string studentName, 
        DateOnly startDate, 
        DateOnly endDate, 
        List<EmailRecipient> recipients, 
        List<Attachment> attachments,
        CancellationToken cancellationToken = default)
    {
        ParentAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StudentName = studentName,
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAttendanceReportEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

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
        SchoolAttendanceReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"ATTN: Attendance Coordinator RE: [Aurora College] Attendance Report {startDate:dd-MM-yyyy}",
            StartDate = startDate,
            EndDate = endDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/SchoolAttendanceReportEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, null, null, null, viewModel.Title, body, attachments, cancellationToken);

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
            StudentFirstName = student.Name.PreferredName,
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceNotificationEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(emailAddresses, string.Empty, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendParentAbsenceDigest(
        string familyName,
        List<AbsenceEntry> wholeAbsences, 
        List<AbsenceEntry> partialAbsences,
        Student student,
        List<EmailRecipient> emailAddresses,
        CancellationToken cancellationToken = default)
    {
        ParentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Absentee Notice - Compulsory School Attendance",
            StudentFirstName = student.Name.PreferredName,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences,
            ParentName = familyName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/ParentAbsenceDigestEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(emailAddresses, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new()
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }

        return null;
    }

    public async Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(
        List<AbsenceEntry> absences, 
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        StudentAbsenceExplanationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name.DisplayName,
            Link = $"https://acos.aurora.nsw.edu.au/Students/Attendance/",
            Absences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceExplanationRequestEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

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
        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        CoordinatorAbsenceVerificationRequestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Partial Absence Verification Request",
            StudentName = student.Name.DisplayName,
            SchoolName = enrolment?.SchoolName ?? "your school",
            ClassList = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceVerificationRequestEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

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

    public async Task<EmailDtos.SentEmail> SendCoordinatorAbsenceDigest(
        List<AbsenceEntry> wholeAbsences, 
        List<AbsenceEntry> partialAbsences, 
        Student student,
        School school,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Count == 0)
            return null;

        CoordinatorAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Request",
            StudentName = student.Name,
            SchoolName = school.Name,
            WholeAbsences = wholeAbsences,
            PartialAbsences = partialAbsences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/CoordinatorAbsenceDigestEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new()
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }

        return null;
    }

    public async Task<EmailDtos.SentEmail> SendStudentAbsenceDigest(
        List<AbsenceEntry> absences,
        Student student,
        List<EmailRecipient> recipients,
        CancellationToken cancellationToken = default)
    {
        if (recipients is null || recipients.Count == 0)
            return null;

        StudentAbsenceDigestEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "[Aurora College] Partial Absentee Notice - Compulsory School Attendance",
            StudentName = student.Name,
            StudentId = student.Id,
            PartialAbsences = absences
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/StudentAbsenceDigestEmail.cshtml", viewModel);

        MimeMessage message = await _emailSender.Send(recipients, string.Empty, viewModel.Title, body, cancellationToken);

        // Perhaps used for future where message file (.eml) is saved to database
        //var messageStream = new MemoryStream();
        //message.WriteTo(messageStream);

        if (message != null)
        {
            return new()
            {
                message = body,
                id = message.MessageId,
                recipients = message.To.ToString()
            };
        }

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
        MissedWorkEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "[Aurora College] Missed Classwork Notification",
            StudentName = student.Name.DisplayName,
            Subject = subjectName,
            ClassName = className,
            AbsenceDate = absenceDate
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/MissedWorkEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.AuroraCollege, viewModel.Title, body, cancellationToken);
    }

    public async Task SendSupportTicketRequest(
        EmailRecipient submitter,
        string subject,
        string description,
        CancellationToken cancellationToken = default)
    {
        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", description);

        await _emailSender.Send([ EmailRecipient.SupportQueue ], submitter, subject, body, cancellationToken);
    }

    public async Task SendAdminAbsenceSentralAlert(string studentName)
    {
        string viewModel = $"<p>{studentName} cannot be located in the Sentral Users list and does not currently have a Sentral Student Id specified.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, "[Aurora College] Student absence notification", body);
    }

    public async Task SendAdminAbsenceContactAlert(string studentName)
    {
        string viewModel = $"<p>Parent contact details for {studentName} cannot be located in Sentral.";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply, "[Aurora College] Constellation Data Issue Identified", body);
    }

    public async Task SendParentContactChangeReportEmail(
        MemoryStream report,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>Parent Contact Change Report for {DateTime.Today.ToLongDateString()} is attached.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege];
        
        List<Attachment> attachments = new()
        {
            new Attachment(report, "Change Report.xlsx", FileContentTypes.ExcelModernFile)
        };

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, $"[Aurora College] Parent Contact Change Report - {DateTime.Today.ToLongDateString()}", body, attachments, cancellationToken);
    }

    public async Task SendAdminLowCreditAlert(double credit)
    {
        string viewModel = $"<p>The SMS Global account has a low balance of ${credit:c}.</p><p>Please top up the account immediately!</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);
        
        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam, EmailRecipient.AuroraCollege];

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, "[Aurora College] SMS Gateway Low Balance Alert", body);
    }

    public async Task SendAssignmentUploadFailedNotification(
        string assignmentName,
        AssignmentId assignmentId,
        string studentName,
        AssignmentSubmissionId submissionId,
        CancellationToken cancellationToken = default)
    {
        string viewModel = $@"<p>Failed to upload an assignment submission to Canvas:</p>
            <dl>
                <dt>Assignment:</dt>
                <dd>{assignmentName} ({assignmentId.Value})</dd>
                <dt>Submission:</dt>
                <dd>From {studentName} ({submissionId.Value})</dd>
            </dl>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> toRecipients = [EmailRecipient.InfoTechTeam];

        await _emailSender.Send(
            toRecipients,
            EmailRecipient.NoReply,
            "[Aurora College] Canvas Assignment Upload Failure",
            body,
            cancellationToken);
    }


    public async Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        AbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            StudentName = notificationEmail.StudentName
        };

        foreach (EmailDtos.AbsenceResponseEmail.AbsenceDto absence in notificationEmail.WholeAbsences)
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

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Absences/AbsenceExplanationToSchoolAdminEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new();
        foreach (string entry in notificationEmail.Recipients)
            if (toRecipients.All(recipient => recipient.Value != entry))
                toRecipients.Add(entry, entry);

        await _emailSender.Send(toRecipients, null, $"Absence Explanation Received - {viewModel.StudentName}", body);
    }

    public async Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail)
    {
        NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = "Absence Explanation Received",
            StudentName = notificationEmail.StudentName
        };

        foreach (EmailDtos.AbsenceResponseEmail.AbsenceDto absence in notificationEmail.WholeAbsences)
        {
            viewModel.Absences.Add(new()
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

        string body = await _razorService.RenderViewToStringAsync(NonResidentialParentAbsenceExplanationToSchoolAdminEmailViewModel.ViewLocation, viewModel);

        Dictionary<string, string> toRecipients = new();
        foreach (string entry in notificationEmail.Recipients)
            if (toRecipients.All(recipient => recipient.Value != entry))
                toRecipients.Add(entry, entry);

        await _emailSender.Send(toRecipients, null, $"Non-Residential Parent Absence Explanation Received - {viewModel.StudentName}", body);
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
        bool singleDayCover = cover.StartDate == cover.EndDate;

        // Send
        NewCoverEmailViewModel viewModel = new()
        {
            ContactName = _configuration.Covers.ContactName,
            ContactPhone = _configuration.Covers.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"Aurora Class Cover - {offering.Name}",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            string summary = $"Aurora College Cover - {offering.Name}";
            string location = $"Class Team ({teamLink})";

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);

            string icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, cancellationToken);
        }

    }

    public async Task SendNewCoverEmail(EmailDtos.CoverEmail resource)
    {
        NewCoverEmailViewModel viewModel = new()
        {
            ToName = resource.CoveringTeacherName,
            Title = "Class Cover Information",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = resource.StartDate,
            EndDate = resource.EndDate,
            HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
            Preheader = "",
            ClassWithLink = resource.ClassesIncluded
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/NewCoverEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new()
        {
            { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
        };

        Dictionary<string, string> ccRecipients = new();

        foreach (KeyValuePair<string, string> entry in resource.ClassroomTeachers)
            if (toRecipients.All(recipient => recipient.Value != entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, string> entry in resource.SecondaryRecipients)
            if (ccRecipients.All(recipient => recipient.Value != entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, EmailRecipient.AuroraCollege.Email, $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
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
        bool singleDayCover = cover.StartDate == cover.EndDate;

        UpdatedCoverEmailViewModel viewModel = new()
        {
            ContactName = _configuration.Covers.ContactName,
            ContactPhone = _configuration.Covers.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"[UPDATED] Aurora Class Cover - {offering.Name}",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{originalStartDate:yyyyMMdd}";
            string summary = $"[UPDATED] Aurora Class Cover - {offering.Name}";
            string location = $"Class Team ({teamLink})";

            // What cycle day does the cover fall on?
            // What periods exist for this class on that cycle day?
            // Extract start and end times for the periods to use in the appointment
            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);

            string icsData = _calendarService.CreateInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, cancellationToken);
        }
    }

    public async Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource)
    {
        UpdatedCoverEmailViewModel viewModel = new()
        {
            ToName = resource.CoveringTeacherName,
            Title = "[UPDATED] Class Cover Information",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = resource.StartDate,
            EndDate = resource.EndDate,
            HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
            Preheader = "",
            ClassWithLink = resource.ClassesIncluded
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/UpdatedCoverEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new()
        {
            { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
        };

        Dictionary<string, string> ccRecipients = new();

        foreach (KeyValuePair<string, string> entry in resource.ClassroomTeachers)
            if (toRecipients.All(recipient => recipient.Value != entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, string> entry in resource.SecondaryRecipients)
            if (ccRecipients.All(recipient => recipient.Value != entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, EmailRecipient.AuroraCollege.Email, $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
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
        bool singleDayCover = cover.StartDate == cover.EndDate;

        // Send
        CancelledCoverEmailViewModel viewModel = new()
        {
            ContactName = _configuration.Covers.ContactName,
            ContactPhone = _configuration.Covers.ContactPhone,
            ToName = coveringTeacher.Name,
            Title = $"Cancelled Aurora Class Cover - {offering.Name}",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = cover.StartDate.ToDateTime(TimeOnly.MinValue),
            EndDate = cover.EndDate.ToDateTime(TimeOnly.MinValue),
            HasAdobeAccount = true,
            Preheader = "",
            ClassWithLink = new Dictionary<string, string> { { "Class Team", teamLink } }
        };

        if (singleDayCover)
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverAppointment.cshtml", viewModel);

            // Create and add ICS files
            string uid = $"{cover.Id}-{cover.OfferingId}-{cover.StartDate:yyyyMMdd}";
            string summary = $"Aurora College Cover - {offering.Name}";
            string location = $"Class Team ({teamLink}";

            DateTime appointmentStart = cover.StartDate.ToDateTime(startTime);
            DateTime appointmentEnd = cover.EndDate.ToDateTime(endTime);
            string icsData = _calendarService.CancelInvite(uid, coveringTeacher.Name, coveringTeacher.Email, summary, location, body, appointmentStart, appointmentEnd, 0);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, icsData, cancellationToken);
        }
        else
        {
            string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

            await _emailSender.Send(primaryRecipients, secondaryRecipients, EmailRecipient.AuroraCollege.Email, viewModel.Title, body, attachments, cancellationToken);
        }
    }

    public async Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource)
    {
        CancelledCoverEmailViewModel viewModel = new()
        {
            ToName = resource.CoveringTeacherName,
            Title = "[CANCELLED] Class Cover Information",
            SenderName = _configuration.Covers.ContactName ?? string.Empty,
            SenderTitle = _configuration.Covers.ContactTitle ?? string.Empty,
            StartDate = resource.StartDate,
            EndDate = resource.EndDate,
            HasAdobeAccount = resource.CoveringTeacherAdobeAccount,
            Preheader = "",
            ClassWithLink = resource.ClassesIncluded
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Covers/CancelledCoverEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new()
        {
            { resource.CoveringTeacherName, resource.CoveringTeacherEmail }
        };

        Dictionary<string, string> ccRecipients = new();

        foreach (KeyValuePair<string, string> entry in resource.ClassroomTeachers)
            if (toRecipients.All(recipient => recipient.Value != entry.Value))
                toRecipients.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, string> entry in resource.SecondaryRecipients)
            if (ccRecipients.All(recipient => recipient.Value != entry.Value))
                ccRecipients.Add(entry.Key, entry.Value);

        await _emailSender.Send(toRecipients, ccRecipients, EmailRecipient.AuroraCollege.Email, $"Class Cover Information - {resource.StartDate.ToShortDateString()}", body, resource.Attachments);
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
        FirstWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Lessons.CoordinatorName,
            SenderTitle = _configuration.Lessons.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/Schools/Dashboard",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FirstWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, _configuration.Lessons.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendSecondLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        SecondWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Lessons.CoordinatorName,
            SenderTitle = _configuration.Lessons.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/Schools/Dashboard",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, _configuration.Lessons.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendThirdLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        SecondWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Lessons.CoordinatorName,
            SenderTitle = _configuration.Lessons.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/Schools/Dashboard",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/SecondWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, _configuration.Lessons.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendFinalLessonWarningEmail(LessonMissedNotificationEmail notification)
    {
        FinalWarningEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Lessons.CoordinatorName,
            SenderTitle = _configuration.Lessons.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            Link = "https://acos.aurora.nsw.edu.au/Schools/Dashboard",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/FinalWarningEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, _configuration.Lessons.CoordinatorEmail, viewModel.Title, body);
    }

    private async Task SendLessonAlertEmail(LessonMissedNotificationEmail notification)
    {
        CoordinatorNotificationEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Lessons.CoordinatorName,
            SenderTitle = _configuration.Lessons.CoordinatorTitle,
            Title = "[Aurora College] Science Practical Lesson Overdue",
            SchoolName = notification.SchoolName,
            Lessons = notification.Lessons
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/CoordinatorNotificationEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, _configuration.Lessons.CoordinatorEmail, viewModel.Title, body);
    }

    public async Task SendStudentLessonCompletedEmail(
        Student student,
        string lessonName,
        string courseName,
        CancellationToken cancellationToken)
    {
        StudentMarkedPresentEmailViewModel viewModel = new()
        {
            Title = $"Congratulations on finishing your Science Prac!",
            SenderName = "Silvia Rudmann",
            SenderTitle = "R/Head Teacher Science and Agriculture",
            Preheader = "",
            StudentName = student.Name.DisplayName,
            LessonTitle = lessonName,
            Subject = courseName
        };

        Dictionary<string, string> toRecipients = new()
        {
            { student.Name.DisplayName, student.EmailAddress.Email }
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Lessons/StudentMarkedPresentEmail.cshtml", viewModel);

        await _emailSender.Send(toRecipients, EmailRecipient.NoReply.Email, viewModel.Title, body, cancellationToken);
    }

    public async Task SendServiceLogEmail(ServiceLogEmail notification)
    {
        string viewModel = $"The following messages were logged by {notification.Source} when it ran today.<br>";
        foreach (string line in notification.Log)
            viewModel += line + "<br>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        List<EmailRecipient> recipients = notification.Recipients;
        if (recipients.All(entry => entry.Email != EmailRecipient.InfoTechTeam.Email))
        {
            recipients.Add(EmailRecipient.InfoTechTeam);
        }
        
        await _emailSender.Send(recipients, EmailRecipient.NoReply, $"[Aurora College] Service Log Output - {notification.Source}", body);
    }

    public async Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, Dictionary<string, string> recipients)
    {
        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated email. No action is required outside of school hours.",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}",
            RollEntries = entries
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/DailyReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, null, viewModel.Title, body);
    }

    public async Task SendNoRollMarkingReport(DateOnly reportDate, Dictionary<string, string> recipients)
    {
        DailyReportEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = _configuration.Absences.AbsenceCoordinatorName,
            SenderTitle = _configuration.Absences.AbsenceCoordinatorTitle,
            Title = $"[Aurora College] Roll Marking Report - {reportDate.ToLongDateString()}"
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/RollMarking/NoReportEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, null, viewModel.Title, body);
    }

    public async Task SendMagicLinkLoginEmail(MagicLinkEmail notification)
    {
        MagicLinkLoginEmailViewModel viewModel = new()
        {
            Preheader = "This is an automated message. Please do not reply.",
            SenderName = "",
            SenderTitle = "",
            Title = "[Aurora College] Portal Login Link",
            ToName = notification.Name,
            Link = notification.Link
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Auth/MagicLinkLoginEmail.cshtml", viewModel);

        await _emailSender.Send(notification.Recipients, EmailRecipient.NoReply, viewModel.Title, body);
    }

    public async Task SendMasterFileConsistencyReportEmail(MemoryStream report, string emailAddress, CancellationToken cancellationToken = default)
    {
        string viewModel = $"<p>MasterFile Consistency Report generated {DateTime.Today.ToLongDateString()} is attached.</p>";

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/PlainEmail.cshtml", viewModel);

        Dictionary<string, string> toRecipients = new()
        {
            { "", emailAddress }
        };

        List<Attachment> attachments = new()
        {
            new Attachment(report, "Consistency Report.xlsx", FileContentTypes.ExcelModernFile)
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
        NewAwardCertificateEmailViewModel viewModel = new()
        {
            Preheader = "",
            SenderName = "",
            SenderTitle = "",
            Title = $"[Aurora College] Student Award for {student.Name.DisplayName}",
            AwardType = award.Type,
            AwardedOn = award.AwardedOn,
            AwardReason = award.Reason,
            StudentName = student?.Name.DisplayName,
            TeacherName = teacher?.DisplayName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Awards/NewAwardCertificateEmail.cshtml", viewModel);
        
        foreach (EmailRecipient recipient in recipients)
        {
            await _emailSender.Send([ recipient ], EmailRecipient.NoReply.Email, viewModel.Title, body, new List<Attachment> { certificate }, cancellationToken);
        }
    }

    public async Task<bool> SendAssignmentUploadReceipt(
        CanvasAssignment assignment,
        CanvasAssignmentSubmission submission,
        Course course,
        Student student,
        SchoolContact contact,
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
            StudentName = student.Name.DisplayName,
            SubmittedOn = DateOnly.FromDateTime(submission.SubmittedOn)
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Assignments/AssignmentSubmissionUploadReceiptEmail.cshtml", viewModel);

        List<EmailRecipient> recipients = new();
        
        Result<EmailRecipient> recipient = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

        if (recipient.IsFailure)
        {
            _logger
                .ForContext(nameof(AssignmentSubmissionUploadReceiptEmailViewModel), viewModel, true)
                .ForContext(nameof(Error), recipient.Error, true)
                .Warning("Failed to send Assignment Upload Receipt");

            return false;
        }

        recipients.Add(recipient.Value);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);

        return true;
    }

    public async Task SendWelcomeEmailToCoordinator(
        List<EmailRecipient> recipients, 
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        NewACCoordinatorEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = "Virginia Cluff",
            SenderTitle = "Instructional Leader",
            Preheader = "",
            PartnerSchool = schoolName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewACCoordinatorEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendWelcomeEmailToSciencePracTeacher(
        List<EmailRecipient> recipients, 
        string schoolName,
        CancellationToken cancellationToken = default)
    {
        NewSciencePracTeacherEmailViewModel viewModel = new()
        {
            Title = $"Welcome to Aurora College!",
            SenderName = "Fiona Boneham",
            SenderTitle = "Science Practical Coordinator",
            Preheader = "",
            PartnerSchool = schoolName
        };

        string body = await _razorService.RenderViewToStringAsync("/Views/Emails/Contacts/NewSciencePracTeacherEmail.cshtml", viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendActionAssignedEmail(
        List<EmailRecipient> recipients,
        Case item,
        Action action,
        Staff assignee,
        CancellationToken cancellationToken = default)
    {
        ActionAssignedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Action Assigned",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            TeacherName = assignee.DisplayName,
            ActionDescription = action.ToString(),
            CaseDescription = item.ToString(),
            Link = $"https://acos.aurora.nsw.edu.au/Staff/SchoolAdmin/WorkFlows/Actions/Update/{item.Id.Value}/{action.Id.Value}"
        };

        string body = await _razorService.RenderViewToStringAsync(ActionAssignedEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendActionCancelledEmail(
        List<EmailRecipient> recipients,
        Case item,
        Action action,
        Staff assignee,
        CancellationToken cancellationToken = default)
    {
        ActionAssignedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Action Cancelled",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            TeacherName = assignee.DisplayName,
            ActionDescription = action.ToString(),
            CaseDescription = item.ToString(),
            Link = $"https://acos.aurora.nsw.edu.au/Staff/SchoolAdmin/WorkFlows/Actions/Update/{item.Id.Value}/{action.Id.Value}"
        };

        string body = await _razorService.RenderViewToStringAsync(ActionCancelledEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendComplianceWorkFlowNotificationEmail(
        List<EmailRecipient> recipients,
        CaseId caseId,
        ComplianceCaseDetail detail,
        int incidentAge,
        string incidentLink,
        CancellationToken cancellationToken = default)
    {
        ComplianceWorkFlowNotificationEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Compliance Case Detected",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            StudentName = detail.Name,
            StudentGrade = detail.Grade.AsName(),
            StudentSchool = detail.SchoolName,
            IncidentType = detail.IncidentType,
            IncidentId = detail.IncidentId,
            Subject = detail.Subject,
            IncidentLink = incidentLink,
            Age = incidentAge,
            Link = $"https://acos.aurora.nsw.edu.au/Staff/SchoolAdmin/WorkFlows/Details/{caseId.Value}"
        };

        string body = await _razorService.RenderViewToStringAsync(ComplianceWorkFlowNotificationEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendTrainingWorkFlowNotificationEmail(
        List<EmailRecipient> recipients,
        TrainingCaseDetail detail,
        string reviewer,
        CancellationToken cancellationToken = default)
    {
        TrainingWorkFlowNotificationEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] Mandatory Training Due",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            StaffName = detail.Name,
            ModuleName = detail.ModuleName,
            DueDate = detail.DueDate,
            DaysUntilDue = detail.DaysUntilDue,
            Reviewer = reviewer
        };

        string body = await _razorService.RenderViewToStringAsync(TrainingWorkFlowNotificationEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendAllActionsCompletedEmail(
        List<EmailRecipient> recipients,
        Case item,
        CancellationToken cancellationToken = default)
    {
        CaseActionsCompletedEmailViewModel viewModel = new()
        {
            Title = $"[WorkFlow] All Case Actions Completed",
            SenderName = "Aurora College",
            SenderTitle = "",
            Preheader = "",
            CaseDescription = item.ToString(),
            Link = $"https://acos.aurora.nsw.edu.au/Staff/SchoolAdmin/WorkFlows/Details/{item.Id.Value}"
        };

        string body = await _razorService.RenderViewToStringAsync(CaseActionsCompletedEmailViewModel.ViewLocation, viewModel);

        await _emailSender.Send(recipients, EmailRecipient.NoReply, viewModel.Title, body, cancellationToken);
    }

    public async Task SendEnteredEmailForAction(
        List<EmailRecipient> recipients,
        EmailRecipient sender,
        string subject,
        string body,
        List<Attachment> attachments,
        CancellationToken cancellationToken = default) =>
        await _emailSender.Send([], [], recipients, sender.Email, subject, body, attachments, cancellationToken);
}
