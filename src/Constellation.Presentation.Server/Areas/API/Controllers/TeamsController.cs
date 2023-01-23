namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Constellation.Application.Teams.ArchiveTeam;
using Constellation.Application.Teams.CreateTeam;
using Constellation.Application.Teams.DeleteTeam;
using Constellation.Application.Teams.GetAllTeams;
using Constellation.Application.Teams.GetTeamById;
using Constellation.Application.Teams.Models;
using Constellation.Presentation.Server.Areas.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[Route("api/v1/[controller]")]
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public TeamsController(IMediator mediator, Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<TeamsController>();
    }

    [HttpGet]
    public async Task<List<TeamResource>> GetAllTeams()
    {
        var teamsRequest = await _mediator.Send(new GetAllTeamsQuery());

        if (teamsRequest.IsSuccess)
        {
            return teamsRequest.Value;
        }

        _logger.Warning("Failed to retrieve all teams with errors {@error}", teamsRequest.Error);

        return null;
    }

    [HttpGet("{id:guid}")]
    public async Task<TeamResource> GetTeam([FromRoute] Guid Id)
    {
        var teamRequest = await _mediator.Send(new GetTeamByIdQuery(Id));

        if (teamRequest.IsSuccess)
        {
            return teamRequest.Value;
        }

        _logger.Warning("Failed to retrieve team {teamId} with errors {@errors}", Id, teamRequest.Error);

        return null;
    }

    [HttpPost("{id:guid}/Archive")]
    public async Task<IActionResult> ArchiveTeam([FromRoute] Guid Id)
    {
        var teamRequest = await _mediator.Send(new ArchiveTeamCommand(Id));

        if (teamRequest.IsSuccess)
        {
            return Ok();
        }

        _logger.Warning("Failed to archived team {teamId} with errors {@errors}", Id, teamRequest.Error);

        return BadRequest();
    }

    [HttpPost("Create")]
    public async Task CreateTeam([FromBody] CreateTeamResource team)
    {
        var command = new CreateTeamCommand(
            team.Id,
            team.Name,
            team.Description,
            team.GeneralChannelId);

        await _mediator.Send(command);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletedTeam([FromRoute] Guid Id)
    {
        var command = new DeleteTeamCommand(Id);

        var response = await _mediator.Send(command);

        if (response.IsSuccess)
        {
            return Ok();
        }

        _logger.Warning("Failed to delete team {teamId} with errors {@errors}", Id, response.Error);

        return BadRequest();
    }
}
