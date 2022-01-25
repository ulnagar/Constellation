using Constellation.Application.Enums;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IOperationService
    {
        Task CreateStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule);
        Task CreateCasualAdobeConnectAccess(int casualId, string roomId, DateTime schedule, int coverId);
        Task CreateTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, int coverId);
        Task RemoveStudentAdobeConnectAccess(string studentId, string roomId, DateTime schedule);
        Task RemoveCasualAdobeConnectAccess(int casualId, string roomId, DateTime schedule, int coverId);
        Task RemoveTeacherAdobeConnectAccess(string staffId, string roomId, DateTime scheduled, int coverId);
        Task CreateTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled);
        Task RemoveTeacherAdobeConnectGroupMembership(string staffId, AdobeConnectGroup groupName, DateTime scheduled);

        Task MarkAdobeConnectOperationComplete(int id);
        Task CancelAdobeConnectOperation(int id);
        void CancelAdobeConnectOperation(AdobeConnectOperation operation);

        Task CreateStudentMSTeamMemberAccess(string studentId, int offeringId, DateTime schedule);
        Task CreateCasualMSTeamMemberAccess(int casualId, int offeringId, int coverId, DateTime scheduled);
        Task CreateTeacherMSTeamMemberAccess(string staffId, int offeringId, DateTime scheduled, int? coverId);
        Task CreateCasualMSTeamOwnerAccess(int casualId, int offeringId, int coverId, DateTime scheduled);
        Task CreateTeacherMSTeamOwnerAccess(string staffId, int offeringId, DateTime scheduled, int? coverId);
        Task CreateClassroomMSTeam(int offeringId, DateTime scheduled);
        Task RemoveStudentMSTeamAccess(string studentId, int offeringId, DateTime schedule);
        Task RemoveCasualMSTeamAccess(int casualId, int offeringId, int coverId, DateTime scheduled);
        Task RemoveTeacherMSTeamAccess(string staffId, int offeringId, DateTime scheduled, int? coverId);

        Task MarkMSTeamOperationComplete(int id);
        Task CancelMSTeamOperation(int id);

        Task CreateStudentEnrolmentMSTeamAccess(string studentId);
        Task RemoveStudentEnrollmentMSTeamAccess(string studentId);
        Task CreateTeacherEmployedMSTeamAccess(string staffId);
        Task RemoveTeacherEmployedMSTeamAccess(string staffId);
        Task CreateContactAddedMSTeamAccess(int contactId);
        Task RemoveContactAddedMSTeamAccess(int contactId);

        Task AddStaffToAdobeGroupBasedOnFaculty(string staffId, Faculty staffFaculty);
        Task AuditStaffAdobeGroupMembershipBasedOnFaculty(string staffId, Faculty originalFaculty, Faculty staffFaculty);
        Task RemoveStaffAdobeGroupMembershipBasedOnFaculty(string staffId, Faculty staffFaculty);

        Task MarkAdobeConnectOperationCompleteAsync(int id);

        Task CreateCanvasUserFromStudent(Student student);
        Task CreateCanvasUserFromStaff(Staff staff);
        Task EnrolStudentInCanvasCourse(Student student, CourseOffering offering, DateTime? scheduledFor = null);
        Task EnrolStaffInCanvasCourse(Staff staff, CourseOffering offering, DateTime? scheduledFor = null);
        Task UnenrolStudentFromCanvasCourse(Student student, CourseOffering offering, DateTime? scheduledFor = null);
        Task UnenrolStaffFromCanvasCourse(Staff staff, CourseOffering offering, DateTime? scheduledFor = null);
        Task DisableCanvasUser(string UserId);
    }
}
