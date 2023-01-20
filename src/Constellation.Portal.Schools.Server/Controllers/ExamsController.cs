namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Portal.School.Assignments.Commands;
using Constellation.Application.Features.Portal.School.Assignments.Models;
using Constellation.Application.Features.Portal.School.Assignments.Queries;
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

    [HttpGet("Courses/{studentId}")]
    public async Task<List<StudentCourseForDropdownSelection>> GetStudentCourses([FromRoute] string studentId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to get courses for student {studentId} to upload exam by user {user}", studentId, user.DisplayName);

        var courses = await _mediator.Send(new GetCoursesForStudentQuery { StudentId = studentId });

        return courses.ToList();
    }

    [HttpGet("Assignments/{courseId}")]
    public async Task<List<StudentAssignmentForCourse>> GetCourseAssignments([FromRoute] int courseId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to get assignments for course {courseId} to upload exam by user {user}", courseId, user.DisplayName);

        var assignments = await _mediator.Send(new GetAssignmentsForCourseQuery { CourseId = courseId });

        return assignments.ToList();
    }

    [HttpPost("Upload")]
    public async Task UploadAssignment([FromBody] UploadStudentAssignmentSubmissionCommand command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to get upload assignment with details {@details} to upload exam by user {user}", command, user.DisplayName);

        await _mediator.Send(command);
    }
}
