namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Schools.GetSchoolsFromList;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Models.Identity;
using Constellation.Application.Schools.GetSchoolContactDetails;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class SchoolsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public SchoolsController(
        IMediator mediator, 
        Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<SchoolsController>();
    }

    [HttpGet]
    public async Task<ApiResult<List<SchoolDto>>> Get()
    {
        AppUser? user = await GetCurrentUser();

        List<string> schoolCodes = new();

        if (await IsUserAdmin(user))
        {
            schoolCodes = (await _mediator.Send(new GetAllPartnerSchoolCodesQuery())).ToList();
        }
        else
        {
            schoolCodes = await GetCurrentUserSchools();
        }

        _logger.Information("Requested to get list of schools for user {user} with return values {@values}", user.DisplayName, schoolCodes);

        Result<List<SchoolDto>>? request = await _mediator.Send(new GetSchoolsFromListQuery(schoolCodes));

        return ApiResult.FromResult(request);
    }

    [HttpGet("{code}/Details")]
    public async Task<ApiResult<SchoolContactDetailsResponse>> GetDetails(string code)
    {
        Result<SchoolContactDetailsResponse>? request = await _mediator.Send(new GetSchoolContactDetailsQuery(code));

        return ApiResult.FromResult(request);
    }

}
