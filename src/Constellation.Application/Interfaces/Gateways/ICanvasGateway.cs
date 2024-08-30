namespace Constellation.Application.Interfaces.Gateways;

using Core.Models.Attachments.DTOs;
using Core.Models.Canvas.Models;
using DTOs;
using DTOs.Canvas;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ICanvasGateway
{
    Task<bool> CreateUser(string userId, string firstName, string lastName, string loginEmail, string userEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing user account for changed email address
    /// </summary>
    /// <param name="userId">SIS_USER_ID: StudentReferenceNumber of StaffId</param>
    /// <param name="emailAddress">string: new EmailAddress of user</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<bool> UpdateUserEmail(string userId, string emailAddress, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUser(string userId, CancellationToken cancellationToken = default);
    Task<bool> EnrolToCourse(string userId, CanvasCourseCode courseId, CanvasPermissionLevel permissionLevel, CancellationToken cancellationToken = default);
    Task<bool> EnrolToSection(string userId, CanvasSectionCode sectionId, CanvasPermissionLevel permissionLevel, CancellationToken cancellationToken = default);
    Task<bool> ReactivateUser(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark all existing enrolments for the user in the Canvas Course as INACTIVE
    /// </summary>
    /// <param name="userId">SIS_USER_ID: StudentReferenceNumber or StaffId</param>
    /// <param name="courseId">SIS_COURSE_ID: CanvasCourseCode</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<bool> UnenrolUser(string userId, CanvasCourseCode courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a single enrolment for the user in the Canvas Course as INACTIVE, specifically targeting a Section enrolment
    /// </summary>
    /// <param name="enrollmentId">int: Canvas Enrollment Id</param>
    /// <param name="courseId">SIS_COURSE_ID: CanvasCourseCode</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<bool> UnenrolUser(int enrollmentId, CanvasCourseCode courseId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUser(string userId, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignmentDto>> GetAllCourseAssignments(CanvasCourseCode courseId, CancellationToken cancellationToken = default);
    Task<List<CanvasAssignmentDto>> GetAllUploadCourseAssignments(CanvasCourseCode courseId, CancellationToken cancellationToken = default);
    Task<RubricEntry> GetCourseAssignmentDetails(CanvasCourseCode courseId, int assignmentId, CancellationToken cancellationToken = default);
    Task<bool> UploadAssignmentSubmission(CanvasCourseCode courseId, int canvasAssignmentId, string studentId, AttachmentResponse file, CancellationToken cancellationToken = default);
    Task<List<AssignmentResultEntry>> GetCourseAssignmentSubmissions(CanvasCourseCode courseId, int assignmentId, CancellationToken cancellationToken = default);

    Task<List<CourseListEntry>> GetAllCourses(string year, CancellationToken cancellationToken = default);
    Task<List<CourseEnrolmentEntry>> GetEnrolmentsForCourse(CanvasCourseCode courseId, CancellationToken cancellationToken = default);
    Task<bool> AddUserToGroup(string userId, CanvasSectionCode groupId, CancellationToken cancellationToken = default);
    Task<List<string>> GetGroupMembers(CanvasSectionCode groupId, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserFromGroup(string userId, CanvasSectionCode groupId, CancellationToken cancellationToken = default);
}