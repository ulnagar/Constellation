namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Attendance.Commands;
using Constellation.Application.Features.Attendance.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class AttendanceController : BaseAPIController
{
    private readonly ILogger<AttendanceController> _logger;
    private readonly IMediator _mediator;

    public AttendanceController(ILogger<AttendanceController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IList<AbsenceDto>> Get()
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve attendance data for parent {name}", user.UserName);

        return await _mediator.Send(new GetAbsencesForFamilyQuery { ParentEmail = user.Email });
    }

    [HttpGet("Details/{id:guid}")]
    public async Task<AbsenceDetailDto> GetDetails([FromRoute] Guid Id)
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve absence details for id {id} by parent {name}", Id, user.UserName);

        // This Mediator Handler is secured so that data is only returned if the parent email matches the absence id.
        return await _mediator.Send(new GetAbsenceDetailsForParentQuery(user.Email, Id));
    }

    [HttpPost("ParentExplanation")]
    public async Task<IActionResult> Explain([FromBody] ProvideParentWholeAbsenceExplanationCommand command)
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to record explanation of absence for id {id} by parent {name}", command.AbsenceId, user.UserName);

        command.ParentEmail = user.Email;

        // This Mediator Handler is secured so that the data is only saved if the parent email matches the absence id.
        await _mediator.Send(command);

        return Ok();
    }

    [HttpGet("Reports/Dates")]
    public async Task<IList<ValidAttendenceReportDate>> GetReportDates()
    {
        return await _mediator.Send(new GetValidAttendenceReportDatesQuery());
    }

    [HttpPost("Reports/Download")]
    public async Task<IActionResult> GetAttendanceReport([FromBody] AttendanceReportRequest request)
    {
        var authorised = await HasAuthorizedAccessToStudent(_mediator, request.StudentId);

        if (!authorised)
        {
            return BadRequest();
        }

        // Create file as stream
        var stream = await _mediator.Send(new GetAttendanceReportForStudentQuery { StudentId = request.StudentId, StartDate = request.StartDate, EndDate = request.EndDate });

        var filename = $"Attendance Report - {request.StartDate:yyyy-MM-dd}.pdf";

        return File(stream, "application/pdf", filename);
    }
}