using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IEmailService
    {
        // Lesson Emails
        Task SendLessonMissedEmail(LessonMissedNotificationEmail notification);


        // Absence Emails
        Task SendAbsenceReasonToSchoolAdmin(EmailDtos.AbsenceResponseEmail notificationEmail);
        Task<Guid> SendCoordinatorPartialAbsenceVerificationRequest(EmailDtos.AbsenceResponseEmail emailDto);
        Task<Guid> SendCoordinatorWholeAbsenceDigest(List<Absence> absences);
        Task<Guid> SendParentWholeAbsenceAlert(List<Absence> absences, List<string> emailAddresses);
        Task<Guid> SendParentWholeAbsenceDigest(List<Absence> absences, List<string> emailAddresses);
        Task<Guid> SendStudentPartialAbsenceExplanationRequest(List<Absence> absences, List<string> emailAddresses);


        // Attendance Emails
        Task SendAttendanceReport(AttendanceReportEmail notification);


        // ClassworkNotification Emails
        Task SendTeacherClassworkNotificationRequest(ClassworkNotificationTeacherEmail notification);
        Task SendStudentClassworkNotification(Absence absence, ClassworkNotification notification, List<string> parentEmails);


        // RollMarking Emails
        Task SendDailyRollMarkingReport(List<RollMarkReportDto> orderedEntries, bool completeReport);



        // Cover Emails
        Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource);
        Task SendNewCoverEmail(EmailDtos.CoverEmail resource);
        Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource);



        // Service Emails
        Task SendServiceLogEmail(ServiceLogEmail notification);
        Task SendAdminAbsenceContactAlert(Student student);
        Task SendAdminAbsenceSentralAlert(Student student);
        Task SendAdminLowCreditAlert(double credit);
    }
}
