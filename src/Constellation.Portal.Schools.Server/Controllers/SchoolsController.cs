namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Schools.GetSchoolsFromList;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Contacts.Models;
using Constellation.Application.Features.Portal.School.Contacts.Queries;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Models.Identity;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class SchoolsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly Serilog.ILogger _logger;

    public SchoolsController(IMediator mediator, Serilog.ILogger logger, UserManager<AppUser> userManager)
	{
        _mediator = mediator;
        _userManager = userManager;
        _logger = logger.ForContext<SchoolsController>();
    }

    [HttpGet]
    public async Task<List<SchoolDto>> Get()
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

        if (request.IsFailure)
        {
            return new List<SchoolDto>();
        }

        return request.Value;
    }

    [HttpGet("{code}/Details")]
    public async Task<SchoolContactDetails> GetDetails(string code)
    {
        return await _mediator.Send(new GetSchoolContactDetailsQuery { Code = code });
    }

}
