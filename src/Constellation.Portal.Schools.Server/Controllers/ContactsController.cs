namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactsWithRoleFromSchool;
using Constellation.Application.SchoolContacts.RequestContactRemoval;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class ContactsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public ContactsController(
        IMediator mediator, 
        Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<SchoolsController>();
    }

    [HttpGet("FromSchool/{code}")]
    public async Task<ApiResult<List<ContactResponse>>> Get(string code)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to get list of school contacts for user {user}", user.DisplayName);

        Result<List<ContactResponse>> contacts = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(code));

        return ApiResult.FromResult(contacts);
    }

    [HttpPost("{id:int}/Remove")]
    public async Task<ApiResult> Remove(
        [FromRoute] int id, 
        [FromBody] RequestContactRemovalCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to remove School Contact by {user} with details {@details}", user.DisplayName, command);

        Result? response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }

    [HttpPost("new")]
    public async Task<ApiResult> CreateNew([FromBody] CreateContactWithRoleCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to create new School Contact by {user} with details {@details}", user.DisplayName, command);

        Result? response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }
}
