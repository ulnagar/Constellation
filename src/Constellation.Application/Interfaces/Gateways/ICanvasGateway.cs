namespace Constellation.Application.Interfaces.Gateways;

using DTOs;
using DTOs.Canvas;
using Core.Models.Attachments.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICanvasGateway
{
    Task<bool> CreateUser(string userId, string firstName, string lastName, string loginEmail, string userEmail, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUser(string userId, CancellationToken cancellationToken = default);
    Task<bool> EnrolToCourse(string userId, string courseId, string permissionLevel, CancellationToken cancellationToken = default);
    Task<bool> EnrolToSection(string userId, string sectionId, string permissionLevel, CancellationToken cancellationToken = default);
    Task<bool> ReactivateUser(string userId, CancellationToken cancellationToken = default);
    Task<bool> UnenrolUser(string userId, string courseId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUser(string userId, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(string courseId, CancellationToken cancellationToken = default);
    Task<bool> UploadAssignmentSubmission(string courseId, int canvasAssignmentId, string studentId, AttachmentResponse file, CancellationToken cancellationToken = default);

    Task<List<CourseListEntry>> GetAllCourses(string year, CancellationToken cancellationToken = default);
    Task<List<CourseEnrolmentEntry>> GetEnrolmentsForCourse(string courseId, CancellationToken cancellationToken = default);
    Task<bool> AddUserToGroup(string userId, string groupId, CancellationToken cancellationToken = default);
    Task<List<string>> GetGroupMembers(string groupId, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserFromGroup(string userId, string groupId, CancellationToken cancellationToken = default);
}