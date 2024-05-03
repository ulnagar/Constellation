namespace Constellation.Application.Interfaces.Gateways;

using Constellation.Application.DTOs;
using Constellation.Application.DTOs.Canvas;
using Core.Models.Attachments.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICanvasGateway
{
    Task<bool> CreateUser(string UserId, string FirstName, string LastName, string LoginEmail, string UserEmail);
    Task<bool> DeactivateUser(string UserId);
    Task<bool> EnrolUser(string UserId, string CourseId, string PermissionLevel);
    Task<bool> ReactivateUser(string UserId);
    Task<bool> UnenrolUser(string UserId, string CourseId);
    Task<bool> DeleteUser(string UserId);
    Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(string CourseId);
    Task<bool> UploadAssignmentSubmission(string CourseId, int CanvasAssignmentId, string StudentId, AttachmentResponse file);

    Task<List<CourseListEntry>> GetAllCourses(string year, CancellationToken cancellationToken = default);
    Task<List<CourseEnrolmentEntry>> GetEnrolmentsForCourse(string courseId, CancellationToken cancellationToken = default);
    Task<bool> AddUserToGroup(string userId, string groupId, CancellationToken cancellationToken = default);
    Task<bool> CheckGroupExists(string groupId, CancellationToken cancellationToken = default);
    Task<bool> CheckGroupCategoryExists(string categoryId, CancellationToken cancellationToken = default);
    Task<bool> CreateGroupCategory(string categoryId, CancellationToken cancellationToken = default);
    Task<bool> CreateGroup(string groupId, CancellationToken cancellationToken = default);
    Task<List<string>> GetGroupMembers(string groupId, CancellationToken cancellationToken = default);
    Task<string> GetUserData(int canvasUserId, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserFromGroup(string userId, string groupId, CancellationToken cancellationToken = default);
}