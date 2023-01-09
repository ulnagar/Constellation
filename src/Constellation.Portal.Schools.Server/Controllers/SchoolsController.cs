using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Contacts.Models;
using Constellation.Application.Features.Portal.School.Contacts.Queries;
using Constellation.Application.Features.Portal.School.Home.Commands;
using Constellation.Application.Features.Portal.School.Home.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Portal.Schools.Server.Controllers;

[Route("api/[controller]")]
public class SchoolsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public SchoolsController(IMediator mediator, Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<SchoolsController>();
    }

    [HttpGet]
    public async Task<List<SchoolDto>> Get()
    {
        var user = await GetCurrentUser();

        var schoolCodes = new List<string>();

        if (await IsUserAdmin(user))
        {
            schoolCodes = await _mediator.Send(new GetAllPartnerSchoolCodesQuery()) as List<string>;
        }
        else
        {
            schoolCodes = GetCurrentUserSchools();
        }

        _logger.Information("Requested to get list of schools for user {user}", user.DisplayName);

        var schoolDtos = await _mediator.Send(new ConvertListOfSchoolCodesToSchoolListCommand { SchoolCodes = schoolCodes });

        return schoolDtos.ToList();
    }

    [HttpGet("{code}/Details")]
    public async Task<SchoolContactDetails> GetDetails(string code)
    {
        return await _mediator.Send(new GetSchoolContactDetailsQuery { Code = code });
    }

}
