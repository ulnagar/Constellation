namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Partners.Schools.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class HomeController : BaseAPIController
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("schools")]
    public async Task<List<SchoolDto>> GetLinkedPartnerSchools()
    {
        var user = await GetCurrentUser();

        return await _mediator.Send(new GetLinkedPartnerSchoolsForUserQuery { Email = user.Email });
    }
}
