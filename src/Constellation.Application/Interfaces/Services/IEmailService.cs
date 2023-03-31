using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IEmailService
    {
        // Lesson Emails
        Task SendLessonMissedEmail(LessonMissedNotificationEmail notification);


        // Absence Emails
        Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail);
        Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(EmailDtos.AbsenceResponseEmail emailDto);
        Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(List<Absence> absences);
        Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<Absence> absences, List<EmailRecipient> emailAddresses);
        Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(List<Absence> absences, List<EmailRecipient> emailAddresses);
        Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<Absence> absences, List<string> emailAddresses);


        // Attendance Emails
        Task<bool> SendAttendanceReport(AttendanceReportEmail notification);


        // ClassworkNotification Emails
        Task SendTeacherClassworkNotificationRequest(ClassworkNotificationTeacherEmail notification);
        Task SendStudentClassworkNotification(Absence absence, ClassworkNotification notification, List<EmailRecipient> parentEmails);
        Task SendTeacherClassworkNotificationCopy(Absence absence, ClassworkNotification notification, Staff teacher);


        // RollMarking Emails
        Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, Dictionary<string, string> recipients);
        Task SendNoRollMarkingReport(DateOnly reportDate, Dictionary<string, string> recipients);


        // Cover Emails
        Task SendCancelledCoverEmail(ClassCover cover, CourseOffering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);
        Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource);
        Task SendNewCoverEmail(ClassCover cover, CourseOffering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);
        Task SendNewCoverEmail(EmailDtos.CoverEmail resource);
        Task SendUpdatedCoverEmail(ClassCover cover, CourseOffering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, DateOnly originalStartDate, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);
        Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource);

        // Service Emails
        Task SendParentContactChangeReportEmail(MemoryStream report, CancellationToken cancellationToken = default);
        Task SendServiceLogEmail(ServiceLogEmail notification);
        Task SendAdminAbsenceContactAlert(string studentName);
        Task SendAdminAbsenceSentralAlert(string studentName);
        Task SendAdminLowCreditAlert(double credit);
        Task SendAdminClassworkNotificationContactAlert(Student student, Staff teacher, ClassworkNotification notification);
        Task SendMasterFileConsistencyReportEmail(MemoryStream report, string emailAddress, CancellationToken cancellationToken = default);
        Task SendAssignmentUploadFailedNotification(string assignmentName, AssignmentId assignmentId, string studentName, AssignmentSubmissionId submissionId, CancellationToken cancellationToken = default);

        // Auth Emails
        Task SendMagicLinkLoginEmail(MagicLinkEmail notification);

        // Training Module Emails
        Task SendTrainingExpiryWarningEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients);
        Task SendTrainingExpiryAlertEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients);
        Task SendTrainingExpiredEmail(Dictionary<string, string> courses, Dictionary<string, string> recipients);
    }
}
