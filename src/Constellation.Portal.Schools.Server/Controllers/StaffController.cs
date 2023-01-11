namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Portal.School.Home.Models;
using Constellation.Application.Features.Portal.School.Home.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class StaffController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public StaffController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<StaffController>();
    }

    [HttpGet("ForDropdown/{schoolCode}")]
    public async Task<List<StaffFromSchoolForDropdownSelection>> GetStaffForDropdown([FromRoute] string schoolCode)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve staff members for school {schoolCode} by user {user}", schoolCode, user.DisplayName);

        var staff = await _mediator.Send(new GetStaffFromSchoolForSelectionQuery { SchoolCode = schoolCode });

        return staff.ToList();
    }
}
