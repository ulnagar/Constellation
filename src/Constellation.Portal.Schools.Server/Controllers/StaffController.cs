namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Application.StaffMembers.Models;
using Constellation.Application.StaffMembers.GetStaffFromSchool;
using Core.Shared;
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
    public async Task<ApiResult<List<StaffSelectionListResponse>>> GetStaffForDropdown([FromRoute] string schoolCode)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve staff members for school {schoolCode} by user {user}", schoolCode, user.DisplayName);

        Result<List<StaffSelectionListResponse>>? staff = await _mediator.Send(new GetStaffFromSchoolQuery(schoolCode));

        return ApiResult.FromResult(staff);
    }
}
