namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class TimetablesController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public TimetablesController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<TimetablesController>();
    }

    [HttpGet("ForStudent/{studentId}")]
    public async Task<StudentTimetableDataDto> GetForSchool([FromRoute] string studentId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Student Timetable for student {student} by user {user}", studentId, user.DisplayName);

        return await _mediator.Send(new GetStudentTimetableDataQuery { StudentId = studentId });
    }

    [HttpPost("Download")]
    public async Task<IActionResult> DownloadReport([FromBody] string studentId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to download Student Timetable for student {student} by user {user}", studentId, user.DisplayName);

        var data = await _mediator.Send(new GetStudentTimetableDataQuery { StudentId = studentId });
        var file = await _mediator.Send(new GetStudentTimetableExportQuery { Data = data });

        return File(file.FileData, file.FileType, file.Name);
    }
}
