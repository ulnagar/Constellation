using Constellation.Application.DTOs;
using Constellation.Application.DTOs.EmailRequests;
using Constellation.Core.Models;
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
        Task<EmailDtos.SentEmail> SendCoordinatorPartialAbsenceVerificationRequest(EmailDtos.AbsenceResponseEmail emailDto);
        Task<EmailDtos.SentEmail> SendCoordinatorWholeAbsenceDigest(List<Absence> absences);
        Task<EmailDtos.SentEmail> SendParentWholeAbsenceAlert(List<Absence> absences, List<string> emailAddresses);
        Task<EmailDtos.SentEmail> SendParentWholeAbsenceDigest(List<Absence> absences, List<string> emailAddresses);
        Task<EmailDtos.SentEmail> SendStudentPartialAbsenceExplanationRequest(List<Absence> absences, List<string> emailAddresses);


        // Attendance Emails
        Task<bool> SendAttendanceReport(AttendanceReportEmail notification);


        // ClassworkNotification Emails
        Task SendTeacherClassworkNotificationRequest(ClassworkNotificationTeacherEmail notification);
        Task SendStudentClassworkNotification(Absence absence, ClassworkNotification notification, List<string> parentEmails);
        Task SendTeacherClassworkNotificationCopy(Absence absence, ClassworkNotification notification, Staff teacher);


        // RollMarking Emails
        Task SendDailyRollMarkingReport(List<RollMarkReportDto> orderedEntries, bool sendToAbsenceCoordinator, bool sendToFacultyHeadTeacher);



        // Cover Emails
        Task SendCancelledCoverEmail(EmailDtos.CoverEmail resource);
        Task SendNewCoverEmail(EmailDtos.CoverEmail resource);
        Task SendUpdatedCoverEmail(EmailDtos.CoverEmail resource);



        // Service Emails
        Task SendServiceLogEmail(ServiceLogEmail notification);
        Task SendAdminAbsenceContactAlert(Student student);
        Task SendAdminAbsenceSentralAlert(Student student);
        Task SendAdminLowCreditAlert(double credit);
        Task SendAdminClassworkNotificationContactAlert(Student student, Staff teacher, ClassworkNotification notification);
    }
}
