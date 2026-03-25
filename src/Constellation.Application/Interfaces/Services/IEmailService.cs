namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Core.Models.Assignments;
using Core.Models.Messaging.Email;
using Core.Models.SchoolContacts;
using Core.Models.StaffMembers;
using Core.Models.Subjects;
using Core.Models.ThirdPartyConsent;
using Core.Models.Tutorials;
using Core.Models.WorkFlow;
using Core.Shared;
using Core.ValueObjects;
using Domains.Attendance.Absences.Commands.ConvertAbsenceToAbsenceEntry;
using Domains.Attendance.Absences.Commands.ConvertResponseToAbsenceExplanation;
using Domains.Compliance.Assessments.Models;
using DTOs;
using DTOs.EmailRequests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Action = Core.Models.WorkFlow.Action;

public interface IEmailService
{
    // Absence Emails
    Task SendAbsenceReasonToSchoolAdmin(AbsenceResponseEmail notificationEmail);
    Task<Result<EmailMessage>> SendCoordinatorPartialAbsenceVerificationRequest(List<AbsenceExplanation> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendStudentAbsenceDigest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendCoordinatorAbsenceDigest(List<AbsenceEntry> wholeAbsences, List<AbsenceEntry> partialAbsences, Student student, School school, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendParentWholeAbsenceAlert(string familyName, List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendParentAbsenceDigest(string familyName, List<AbsenceEntry> wholeAbsences, List<AbsenceEntry> partialAbsences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendStudentPartialAbsenceExplanationRequest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task SendNonResidentialParentAbsenceReasonToSchoolAdmin(AbsenceResponseEmail notificationEmail);
    Task SendMissedWorkEmail(Student student, string subjectName, string className, DateOnly absenceDate, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    
    // App Emails
    Task SendParentContactChangeReportEmail(MemoryStream report, CancellationToken cancellationToken = default);
    Task SendServiceLogEmail(ServiceLogEmail notification);
    Task SendAdminAbsenceContactAlert(string studentName);
    Task SendAdminAbsenceSentralAlert(string studentName);
    Task SendAdminLowCreditAlert(double credit);
    Task SendMasterFileConsistencyReportEmail(MemoryStream report, string emailAddress, CancellationToken cancellationToken = default);

    // Assessment Provisions Emails
    Task<Result> SendAssessmentProvisionEmailToSchools(List<EmailRecipient> recipients, List<EmailRecipient> ccRecipients, Name contact, List<StudentProvisions> students, CancellationToken cancellationToken = default);
    Task<Result> SendAssessmentProvisionEmailToFamilies(List<EmailRecipient> recipients, List<EmailRecipient> ccRecipients, StudentProvisions provisions, CancellationToken cancellationToken = default);

    // Assignment Emails
    Task<Result> SendAssignmentUploadReceipt(CanvasAssignment assignment, CanvasAssignmentSubmission submission, Course course, Student student, SchoolContact contact, CancellationToken cancellationToken = default);
    Task SendAssignmentUploadFailedNotification(string assignmentName, AssignmentId assignmentId, string studentName, AssignmentSubmissionId submissionId, CancellationToken cancellationToken = default);

    // Attendance Emails
    Task<Result> SendParentAttendanceReportEmail(string studentName, DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<Result> SendSchoolAttendanceReportEmail(DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<Attachment> attachments, CancellationToken cancellationToken = default);

    // Attendance Plan Emails
    Task SendAttendancePlanToAdmin(List<EmailRecipient> recipients, AttendancePlan plan, CancellationToken cancellationToken = default);
    Task SendAttendancePlanRejectedNotificationToSchool(List<EmailRecipient> recipients, AttendancePlan plan, string comment, CancellationToken cancellationToken = default);

    // Auth Emails
    Task SendMagicLinkLoginEmail(MagicLinkEmail notification);

    // Awards Emails
    Task SendAwardCertificateParentEmail(List<EmailRecipient> recipients, AttachmentResponse certificate, StudentAward award, Student student, StaffMember teacher, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendAwardNominationNotificationEmailToParents(List<EmailRecipient> recipients, List<EmailRecipient> ccRecipients, Name parent, Name student, string school, DateOnly deliveryDate, List<Nomination> awards, CancellationToken cancellationToken = default);
    Task<Result<EmailMessage>> SendAwardNominationNotificationEmailToSchools(List<EmailRecipient> recipients, List<EmailRecipient> ccRecipients, string school, DateOnly deliveryDate, Dictionary<Name, List<Nomination>> students, CancellationToken cancellationToken = default);

    // Cover Emails
    Task SendCancelledCoverEmail(Cover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendNewCoverEmail(Cover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendUpdatedCoverEmail(Cover cover, Offering offering, EmailRecipient coveringTeacher, List<EmailRecipient> primaryRecipients, List<EmailRecipient> secondaryRecipients, DateOnly originalStartDate, TimeOnly startTime, TimeOnly endTime, string teamLink, List<Attachment> attachments, CancellationToken cancellationToken = default);

    // Emergency Console Emails
    Task<Result> SendEmergencyConsoleEmail(AlertRecipient recipient, string message, CancellationToken cancellationToken = default);

    // RollMarking Emails
    Task SendDailyRollMarkingReport(List<RollMarkingEmailDto> entries, DateOnly reportDate, List<EmailRecipient> recipients);
    Task SendNoRollMarkingReport(DateOnly reportDate, List<EmailRecipient> recipients);

    // Scheduled Reports Emails
    Task ForwardCompletedScheduledReport(EmailRecipient recipient, Attachment attachment, CancellationToken cancellationToken = default);

    // School Contact Emails
    Task SendWelcomeEmailToCoordinator(List<EmailRecipient> recipients, string schoolName, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailToSciencePracTeacher(List<EmailRecipient> recipients, string schoolName, CancellationToken cancellationToken = default);
    Task<Result> SendSchoolContactRemovalRequest(SchoolContact contact, SchoolContactRole role, string cancelledBy, string cancelledAt, string comment);
    Task SendSchoolContactAddedNotification(SchoolContact contact, SchoolContactRole role);

    // Science Prac Emails
    Task SendLessonMissedEmail(LessonMissedNotificationEmail notification);
    Task SendStudentLessonCompletedEmail(Student student, string lessonName, string courseName, CancellationToken cancellationToken);

    // Student Portal Emails
    Task SendSupportTicketRequest(EmailRecipient submitter, string subject, string description, CancellationToken cancellationToken = default);

    // Student Report Emails
    Task SendAcademicReportToNonResidentialParent(List<EmailRecipient> recipients, Name studentName, string ReportingPeriod, string Year, FileDto file, CancellationToken cancellationToken = default);

    // Third Party Consent Emails
    Task SendConsentTransactionReceiptToParent(List<EmailRecipient> recipients, string studentName, DateOnly submittedOn, Attachment attachment, CancellationToken cancellationToken = default);
    Task SendConsentRefusedNotification(List<EmailRecipient> recipients, string studentName, DateOnly submittedOn, List<Transaction.ConsentResponse> responses, CancellationToken cancellationToken = default);

    // Tutorial Emails
    Task<Result> SendTutorialRequestReceivedEmail(List<EmailRecipient> recipients, Request tutorialRequest, CancellationToken cancellationToken = default);
    Task<Result> SendTutorialRequestReceivedNotificationEmail(List<EmailRecipient> recipients, Request tutorialRequest, CancellationToken cancellationToken = default);
    Task<Result> SendTutorialRequestApprovedNotificationEmail(List<EmailRecipient> recipients, Request tutorialRequest, CancellationToken cancellationToken = default);
    Task<Result> SendTutorialRequestRejectedEmail(List<EmailRecipient> recipients, Request tutorialRequest, CancellationToken cancellationToken = default);
    Task<Result> SendTutorialRequestScheduledEmail(List<EmailRecipient> recipients, Request tutorialRequest, string teamName, List<(string Period, string Teacher)> periods, DateOnly startDate, CancellationToken cancellationToken = default);

    // WorkFlow Emails
    Task SendActionAssignedEmail(List<EmailRecipient> recipients, Case item, Action action, StaffMember assignee, CancellationToken cancellationToken = default);
    Task SendActionCancelledEmail(List<EmailRecipient> recipients, Case item, Action action, StaffMember assignee, CancellationToken cancellationToken = default);
    Task SendEnteredEmailForAction(List<EmailRecipient> recipients, EmailRecipient sender, string subject, string body, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task SendComplianceWorkFlowNotificationEmail(List<EmailRecipient> recipients, CaseId caseId, Name assignee, ComplianceCaseDetail detail, int incidentAge, string incidentLink, CancellationToken cancellationToken = default);
    Task SendTrainingWorkFlowNotificationEmail(List<EmailRecipient> recipients, TrainingCaseDetail detail, CancellationToken cancellationToken = default);
    Task SendAllActionsCompletedEmail(List<EmailRecipient> recipients, Case item, CancellationToken cancellationToken = default);

}
