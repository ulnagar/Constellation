namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Application.Timetables.GetStudentTimetableData;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Core.Models.Attachments;
using Core.Shared;
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
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Student Timetable for student {student} by user {user}", studentId, user.DisplayName);

        Result<StudentTimetableDataDto> request = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (request.IsFailure)
        {
            return new StudentTimetableDataDto();
        }

        return request.Value;
    }

    [HttpPost("Download")]
    public async Task<IActionResult> DownloadReport([FromBody] string studentId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to download Student Timetable for student {student} by user {user}", studentId, user.DisplayName);

        Result<StudentTimetableDataDto>? request = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (request.IsFailure)
        {
            return BadRequest();
        }

        FileDto? file = await _mediator.Send(new GetStudentTimetableExportQuery { Data = request.Value });

        return File(file.FileData, file.FileType, file.FileName);
    }
}
