namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Application.Stocktake.Models;
using Constellation.Application.Stocktake.CancelSighting;
using Constellation.Application.Stocktake.GetCurrentStocktakeEvents;
using Constellation.Application.Stocktake.GetStocktakeSightingsForSchool;
using Constellation.Application.Stocktake.RegisterSighting;
using Core.Shared;
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
    public async Task<ApiResult<List<StocktakeEventResponse>>> GetEvents()
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve list of current stocktake events by user {user}", user.DisplayName);

        Result<List<StocktakeEventResponse>>? events = await _mediator.Send(new GetCurrentStocktakeEventsQuery());

        return ApiResult.FromResult(events);
    }

    [HttpGet("Events/{eventId:guid}/Sightings/{schoolCode}")]
    public async Task<ApiResult<List<StocktakeSightingResponse>>> GetSightings(
        [FromRoute] Guid eventId, 
        [FromRoute] string schoolCode)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve list of sightings for school {code} for event {event} by user {user}", schoolCode, eventId, user.DisplayName);

        Result<List<StocktakeSightingResponse>>? sightings = await _mediator.Send(new GetStocktakeSightingsForSchoolQuery(schoolCode, eventId));

        return ApiResult.FromResult(sightings);
    }

    [HttpPost("{id:guid}/Remove")]
    public async Task<ApiResult> Remove(
        [FromRoute] Guid stocktakeId, 
        [FromBody] CancelSightingCommand command)
    {
        AppUser? user = await GetCurrentUser();

        CancelSightingCommand completeCommand = command with { CancelledBy = user.Email };

        _logger.Information("Requested to remove Stocktake Sighting by {user} with details {@details}", user.DisplayName, command);

        Result? response = await _mediator.Send(completeCommand);

        return ApiResult.FromResult(response);
    }

    [HttpPost("Submit")]
    public async Task<ApiResult> SubmitSighting(
        [FromBody] RegisterSightingCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Submitting stocktake sighting by user {user} with details {@details}", user.DisplayName, command);

        Result? response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }
}
