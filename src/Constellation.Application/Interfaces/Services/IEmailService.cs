﻿namespace Constellation.Application.Interfaces.Services;

using Constellation.Application.Absences.ConvertAbsenceToAbsenceEntry;
using Constellation.Application.Absences.ConvertResponseToAbsenceExplanation;
using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

public interface IEmailService
{
    // Awards Emails
    Task SendAwardCertificateParentEmail(List<EmailRecipient> recipients, StoredFile certificate, StudentAward award, Student? student, Staff? teacher, CancellationToken cancellationToken = default);

    // Lesson Emails
    Task SendLessonMissedEmail(LessonMissedNotificationEmail notification);


    // Absence Emails
    Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail);
    Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(List<AbsenceExplanation> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> emailAddresses, CancellationToken cancellationToken = default);
    Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<AbsenceEntry> absences, Student student, List<EmailRecipient> recipients, CancellationToken cancellationToken = default);


    // Attendance Emails
    Task<bool> SendParentAttendanceReportEmail(string studentName, DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<Attachment> attachments, CancellationToken cancellationToken = default);
    Task<bool> SendSchoolAttendanceReportEmail(DateOnly startDate, DateOnly endDate, List<EmailRecipient> recipients, List<Attachment> attachments, CancellationToken cancellationToken = default);


    // ClassworkNotification Emails
    Task SendTeacherClassworkNotificationRequest(string offeringName, ClassworkNotification notification, List<Student> students, CancellationToken cancellationToken = default);
    Task SendStudentClassworkNotification(ClassworkNotification notification, string offeringName, string courseName, Student student, Staff teacher, List<EmailRecipient> parentEmails, bool isExplained, CancellationToken cancellationToken = default);
    Task SendTeacherClassworkNotificationCopy(ClassworkNotification notification, string offeringName, string courseName, Student student, Staff teacher, CancellationToken cancellationToken = default);


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

    // Report Emails
    Task SendAcademicReportToNonResidentialParent(List<EmailRecipient> recipients, Name studentName, string ReportingPeriod, string Year, FileDto file, CancellationToken cancellationToken = default);
}
