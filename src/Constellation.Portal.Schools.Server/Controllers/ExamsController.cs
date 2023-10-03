namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Constellation.Application.Assignments.GetAssignmentsByCourse;
using Constellation.Application.Assignments.UploadAssignmentSubmission;
using Constellation.Application.Courses.GetCoursesForStudent;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class ExamsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public ExamsController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<ExamsController>();
    }

    [HttpGet("{studentId}/Courses")]
    public async Task<List<StudentCourseResponse>> GetStudentCourses([FromRoute] string studentId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to get courses for student {studentId} to upload exam by user {user}", studentId, user.DisplayName);

        Result<List<StudentCourseResponse>> coursesRequest = await _mediator.Send(new GetCoursesForStudentQuery(studentId));

        if (coursesRequest.IsFailure)
        {
            return new List<StudentCourseResponse>();
        }

        return coursesRequest.Value;
    }

    [HttpGet("{studentId}/{courseId}/Assignments")]
    public async Task<List<CourseAssignmentResponse>> GetCourseAssignments([FromRoute] string studentId, [FromRoute] Guid courseId)
    {
        AppUser? user = await GetCurrentUser();

        CourseId course = CourseId.FromValue(courseId);

        _logger.Information("Requested to get assignments for course {courseId} to upload exam by user {user}", course, user.DisplayName);

        Result<List<CourseAssignmentResponse>>? assignments = await _mediator.Send(new GetAssignmentsByCourseQuery(course, studentId));

        if (assignments.IsFailure)
        {
            return new List<CourseAssignmentResponse>();
        }

        return assignments.Value;
    }

    [HttpPost("Upload")]
    public async Task<bool> UploadAssignment([FromBody] UploadAssignmentSubmissionCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to get upload assignment with details {@details} to upload exam by user {user}", command, user.DisplayName);

        command.SubmittedBy = user.Email;

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
            return false;

        return true;
    }
}
