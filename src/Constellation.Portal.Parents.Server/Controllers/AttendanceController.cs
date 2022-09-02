namespace Constellation.Portal.Parents.Server.Controllers;

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

    [HttpGet]
    public async Task<AbsenceDetailDto> GetDetails(Guid Id)
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve absence details for id {id} by parent {name}", Id, user.UserName);

        return await _mediator.Send(new GetAbsenceDetailsForParentQuery { AbsenceId = Id });
    }
}