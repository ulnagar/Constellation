namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Timetables.GetStudentTimetableData;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class TimetablesController : BaseAPIController
{
	private readonly ILogger<TimetablesController> _logger;
	private readonly IMediator _mediator;

	public TimetablesController(ILogger<TimetablesController> logger, IMediator mediator)
	{
		_logger = logger;
		_mediator = mediator;
	}

	[HttpGet("{studentId}")]
	public async Task<StudentTimetableDataDto> GetStudentTimetable([FromRoute] string studentId)
	{
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
		{
			return new StudentTimetableDataDto();
		}

		Result<StudentTimetableDataDto> request = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (request.IsFailure)
        {
            return new StudentTimetableDataDto();
        }

        return request.Value;
    }

    [HttpPost("Download/{studentId}")]
    public async Task<IActionResult> GetAttendanceReport([FromRoute] string studentId)
    {
        var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

        if (!authorised)
        {
            return BadRequest();
        }

        // Get Timetable Data first
        var data = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (data.IsFailure)
        {
            return BadRequest();
        }

        // We only have one student, so just download that file.
        var file = await _mediator.Send(new GetStudentTimetableExportQuery { Data = data.Value });

        return File(file.FileData, file.FileType, file.Name);
    }
}
