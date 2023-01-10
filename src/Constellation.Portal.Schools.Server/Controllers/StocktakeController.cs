namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Portal.School.Stocktake.Commands;
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


    [HttpPost("{id:guid}/Remove")]
    public async Task Remove([FromRoute] Guid StocktakeId, [FromBody] CancelStocktakeSightingCommand Command)
    {
        var user = await GetCurrentUser();

        Command.CancelledBy = user.Email;

        _logger.Information("Requested to remove Stocktake Sighting by {user} with details {@details}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }
}
