using Constellation.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    using Core.Models.Attachments;

    public interface ICanvasGateway
    {
        Task<bool> CreateUser(string UserId, string FirstName, string LastName, string LoginEmail, string UserEmail);
        Task<bool> DeactivateUser(string UserId);
        Task<bool> EnrolUser(string UserId, string CourseId, string PermissionLevel);
        Task<bool> ReactivateUser(string UserId);
        Task<bool> UnenrolUser(string UserId, string CourseId);
        Task<bool> DeleteUser(string UserId);
        Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(string CourseId);
        Task<bool> UploadAssignmentSubmission(string CourseId, int CanvasAssignmentId, string StudentId, Attachment file);
    }
}
