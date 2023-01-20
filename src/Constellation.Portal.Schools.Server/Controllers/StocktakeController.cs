namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Portal.School.Stocktake.Commands;
using Constellation.Application.Features.Portal.School.Stocktake.Models;
using Constellation.Application.Features.Portal.School.Stocktake.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class StocktakeController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public StocktakeController(IMediator mediator, Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<StocktakeController>();
    }

    [HttpGet("Events")]
    public async Task<List<StocktakeEventsForList>> GetEvents()
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve list of current stocktake events by user {user}", user.DisplayName);

        var events = await _mediator.Send(new GetCurrentStocktakeEventsQuery());

        return events.ToList();
    }

    [HttpGet("Events/{eventId:guid}/Sightings/{schoolCode}")]
    public async Task<List<StocktakeSightingsForList>> GetSightings([FromRoute] Guid eventId, [FromRoute] string schoolCode)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve list of sightings for school {code} for event {event} by user {user}", schoolCode, eventId, user.DisplayName);

        var sightings = await _mediator.Send(new GetStocktakeSightingsForSchoolQuery { SchoolCode = schoolCode, StocktakeEvent = eventId });

        return sightings.ToList();
    }

    [HttpPost("{id:guid}/Remove")]
    public async Task Remove([FromRoute] Guid StocktakeId, [FromBody] CancelStocktakeSightingCommand Command)
    {
        var user = await GetCurrentUser();

        Command.CancelledBy = user.Email;

        _logger.Information("Requested to remove Stocktake Sighting by {user} with details {@details}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("Submit")]
    public async Task SubmitSighting([FromBody] RegisterSightedDeviceForStocktakeCommand command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Submitting stocktake sighting by user {user} with details {@details}", user.DisplayName, command);

        await _mediator.Send(command);
    }
}
