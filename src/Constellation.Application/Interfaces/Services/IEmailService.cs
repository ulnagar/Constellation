namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.Assignments;
using Core.Models.SchoolContacts;
using Core.Models.Subjects;
using Core.Models.ThirdPartyConsent;
using Core.Models.WorkFlow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

public interface IEmailService
{
    // Awards Emails
    Task SendAwardCertificateParentEmail(List<EmailRecipient> recipients, Attachment certificate, StudentAward award, Student? student, Staff? teacher, CancellationToken cancellationToken = default);

    // Lesson Emails
    Task SendLessonMissedEmail(LessonMissedNotificationEmail notification);
    Task SendStudentLessonCompletedEmail(Student student, string lessonName, string courseName, CancellationToken cancellationToken);

    // Absence Emails
    Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail);
    Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(List<AbsenceExplanation> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendStudentAbsenceDigest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendCoordinatorAbsenceDigest(List<AbsenceEntry> wholeAbsences, List<AbsenceEntry> partialAbsences, Student student, School school, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentAbsenceDigest(string familyName, List<AbsenceEntry> wholeAbsences, List<AbsenceEntry> partialAbsences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail);


    /// <summary>
    /// Send an email to a student requesting they catch up on missed work for the day
    /// </summary>
    /// <param name="student"></param>
    /// <param name="recipients"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMissedWorkEmail(Student student, string subjectName, string className, DateOnly absenceDate, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);

    // Assignment Emails
    Task<bool> SendAssignmentUploadReceipt(CanvasAssignment assignment, CanvasAssignmentSubmission submission, Course course, Student student, SchoolContact contact, CancellationToken cancellationToken = default);


    // Attendance Emails
    Task<bool> SendParentAttendanceReportEmail(string studentName, DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default);
    Task<bool> SendSchoolAttendanceReportEmail(DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default);


    // RollMarking Emails
    Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, Dictionary<string, string> recipients);
    Task SendNoRollMarkingReport(DateOnly reportDate, Dictionary<string, string> recipients);


    // Cover Emails
    Task SendCancelledCoverEmail(ClassCover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource);
    Task SendNewCoverEmail(ClassCover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendNewCoverEmail(EmailDtos.CoverEmail resource);
    Task SendUpdatedCoverEmail(ClassCover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, DateOnly originalStartDate, TimeOnly startTime, TimeOnly endTime, string teamLink, List<System.Net.Mail.Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource);

    // Service Emails
    Task SendParentContactChangeReportEmail(MemoryStream report, CancellationToken cancellationToken = default);
    Task SendServiceLogEmail(ServiceLogEmail notification);
    Task SendAdminAbsenceContactAlert(string studentName);
    Task SendAdminAbsenceSentralAlert(string studentName);
    Task SendAdminLowCreditAlert(double credit);
    Task SendMasterFileConsistencyReportEmail(MemoryStream report, string emailAddress, CancellationToken cancellationToken = default);
    Task SendAssignmentUploadFailedNotification(string assignmentName, AssignmentId assignmentId, string studentName, AssignmentSubmissionId submissionId, CancellationToken cancellationToken = default);

    // Auth Emails
    Task SendMagicLinkLoginEmail(MagicLinkEmail notification);

    // Report Emails
    Task SendAcademicReportToNonResidentialParent(List<EmailRecipient> recipients, Name studentName, string ReportingPeriod, string Year, FileDto file, CancellationToken cancellationToken = default);

    // School Contact Emails
    Task SendWelcomeEmailToCoordinator(List<EmailRecipient> recipients, string schoolName, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailToSciencePracTeacher(List<EmailRecipient> recipients, string schoolName, CancellationToken cancellationToken = default);

    // WorkFlow Emails
    Task SendActionAssignedEmail(List<EmailRecipient> recipients, Case item, Action action, Staff assignee, CancellationToken cancellationToken = default);
    Task SendActionCancelledEmail(List<EmailRecipient> recipients, Case item, Action action, Staff assignee, CancellationToken cancellationToken = default);
    Task SendEnteredEmailForAction(List<EmailRecipient> recipients, EmailRecipient sender, string subject, string body, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendComplianceWorkFlowNotificationEmail(List<EmailRecipient> recipients, CaseId caseId, ComplianceCaseDetail detail, int incidentAge, string incidentLink, CancellationToken cancellationToken = default);
    Task SendTrainingWorkFlowNotificationEmail(List<EmailRecipient> recipients, TrainingCaseDetail detail, string reviewer, CancellationToken cancellationToken = default);
    Task SendAllActionsCompletedEmail(List<EmailRecipient> recipients, Case item, CancellationToken cancellationToken = default);

    // Student Portal Emails
    Task SendSupportTicketRequest(EmailRecipient submitter, string subject, string description, CancellationToken cancellationToken = default);

    // Third Party Consent Emails
    Task SendConsentTransactionReceiptToParent(List<EmailRecipient> recipients, string studentName, DateOnly submittedOn, Attachment attachment, CancellationToken cancellationToken = default);
    Task SendConsentRefusedNotification(List<EmailRecipient> recipients, string studentName, DateOnly submittedOn, List<Transaction.ConsentResponse> responses, CancellationToken cancellationToken = default);

    // Attendance Plan Emails
    Task SendAttendancePlanToAdmin(List<EmailRecipient> recipients, AttendancePlan plan, CancellationToken cancellationToken = default);
    Task SendAttendancePlanRejectedNotificationToSchool(List<EmailRecipient> recipients, AttendancePlan plan, string comment, CancellationToken cancellationToken = default);
}
