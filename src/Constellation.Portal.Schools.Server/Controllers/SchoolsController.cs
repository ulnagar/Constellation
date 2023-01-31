namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Contacts.Models;
using Constellation.Application.Features.Portal.School.Contacts.Queries;
using Constellation.Application.Features.Portal.School.Home.Commands;
using Constellation.Application.Features.Portal.School.Home.Queries;
using Constellation.Application.Models.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

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
        var user = await GetCurrentUser();

        var schoolCodes = new List<string>();

        if (await IsUserAdmin(user))
        {
            schoolCodes = await _mediator.Send(new GetAllPartnerSchoolCodesQuery()) as List<string>;
        }
        else
        {
            schoolCodes = await GetCurrentUserSchools();
        }

        _logger.Information("Requested to get list of schools for user {user} with return values {@values}", user.DisplayName, schoolCodes);

        var schoolDtos = await _mediator.Send(new ConvertListOfSchoolCodesToSchoolListCommand { SchoolCodes = schoolCodes });

        return schoolDtos.ToList();
    }

    [HttpGet("{code}/Details")]
    public async Task<SchoolContactDetails> GetDetails(string code)
    {
        return await _mediator.Send(new GetSchoolContactDetailsQuery { Code = code });
    }

}
