﻿namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
using Constellation.Core.Models.Assignments.Identifiers;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Core.Models.Assignments;
using Core.Models.Subjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

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
    Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(List<AbsenceEntry> absences, Student student, School school, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send an email to a student requesting they catch up on missed work for the day
    /// </summary>
    /// <param name="student"></param>
    /// <param name="recipients"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendMissedWorkEmail(Student student, string subjectName, string className, DateOnly absenceDate, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);

    // Assignment Emails
    Task<bool> SendAssignmentUploadReceipt(CanvasAssignment assignment, CanvasAssignmentSubmission submission, Course course, Student student, CancellationToken cancellationToken = default);


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

    // Training Module Emails
    Task SendTrainingExpiryWarningEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients);
    Task SendTrainingExpiryAlertEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients);
    Task SendTrainingExpiredEmail(Dictionary<string, string> courses, List<EmailRecipient> recipients);
        
    // Report Emails
    Task SendAcademicReportToNonResidentialParent(List<EmailRecipient> recipients, Name studentName, string ReportingPeriod, string Year, FileDto file, CancellationToken cancellationToken = default);
}
