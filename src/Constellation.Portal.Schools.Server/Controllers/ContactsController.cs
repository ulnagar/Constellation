namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Partners.SchoolContacts.Models;
using Constellation.Application.Features.Partners.SchoolContacts.Queries;
using Constellation.Application.Features.Portal.School.Contacts.Commands;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class ContactsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public ContactsController(IMediator mediator, Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<SchoolsController>();
    }

    [HttpGet("FromSchool/{code}")]
    public async Task<List<ContactWithRoleForList>> Get(string code)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to get list of school contacts for user {user}", user.DisplayName);

        var contacts = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery { Code = code });

        return contacts.ToList();
    }

    [HttpPost("{id}/Remove")]
    public async Task Remove([FromRoute] int ContactId, [FromBody] RequestRemovalOfSchoolContactCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to remove School Contact by {user} with details {@details}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("new")]
    public async Task Createnew([FromBody] CreateContactWithRoleCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to create new School Contact by {user} with details {@details}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }
}
