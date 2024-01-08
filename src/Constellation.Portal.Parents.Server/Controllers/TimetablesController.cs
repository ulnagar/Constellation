namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Timetables.GetStudentTimetableData;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class TimetablesController : BaseAPIController
{
	private readonly Serilog.ILogger _logger;
	private readonly IMediator _mediator;

	public TimetablesController(
        Serilog.ILogger logger, 
        IMediator mediator)
	{
		_logger = logger.ForContext<TimetablesController>();
		_mediator = mediator;
	}

	[HttpGet("{studentId}")]
	public async Task<ApiResult<StudentTimetableDataDto>> GetStudentTimetable([FromRoute] string studentId)
	{
		bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
            return ApiResult.FromResult(Result.Failure<StudentTimetableDataDto>(DomainErrors.Auth.NotAuthorised));

		Result<StudentTimetableDataDto> request = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        return ApiResult.FromResult(request);
    }

    [HttpPost("Download/{studentId}")]
    public async Task<IActionResult> GetAttendanceReport([FromRoute] string studentId)
    {
        bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

        if (!authorised)
        {
            return Ok(ApiResult.FromResult(Result.Failure(DomainErrors.Auth.NotAuthorised)));
        }

        // Get Timetable Data first
        Result<StudentTimetableDataDto>? data = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (data.IsFailure)
        {
            return Ok(ApiResult.FromResult(data));
        }

        // We only have one student, so just download that file.
        FileDto? file = await _mediator.Send(new GetStudentTimetableExportQuery { Data = data.Value });

        return File(file.FileData, file.FileType, file.FileName);
    }
}
