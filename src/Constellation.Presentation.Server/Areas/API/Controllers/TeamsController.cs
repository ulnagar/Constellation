namespace Constellation.Presentation.Server.Areas.API.Controllers;

using Application.Domains.LinkedSystems.Teams.Commands.ArchiveTeam;
using Application.Domains.LinkedSystems.Teams.Commands.CreateTeam;
using Application.Domains.LinkedSystems.Teams.Commands.DeleteTeam;
using Application.Domains.LinkedSystems.Teams.Models;
using Application.Domains.LinkedSystems.Teams.Queries.GetAllTeams;
using Application.Domains.LinkedSystems.Teams.Queries.GetTeamById;
using Application.Domains.LinkedSystems.Teams.Queries.GetTeamMembershipById;
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

    [Produces(typeof(List<TeamResource>))]
    [HttpGet]
    public async Task<IActionResult> GetAllTeams()
    {
        var teamsRequest = await _mediator.Send(new GetAllTeamsQuery());

        if (teamsRequest.IsSuccess)
        {
            return Ok(teamsRequest.Value);
        }

        _logger.Warning("Failed to retrieve all teams with errors {@error}", teamsRequest.Error);

        return BadRequest();
    }

    [Produces(typeof(TeamResource))]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTeam([FromRoute] Guid Id)
    {
        var teamRequest = await _mediator.Send(new GetTeamByIdQuery(Id));

        if (teamRequest.IsSuccess)
        {
            return Ok(teamRequest.Value);
        }

        _logger.Warning("Failed to retrieve team {teamId} with errors {@errors}", Id, teamRequest.Error);

        return BadRequest();
    }

    [Produces(typeof(List<TeamMembershipResponse>))]
    [HttpGet("{id:guid}/Members")]
    public async Task<IActionResult> GetTeamMembership([FromRoute] Guid Id)
    {
        var teamMemberRequest = await _mediator.Send(new GetTeamMembershipByIdQuery(Id));

        if (teamMemberRequest.IsSuccess)
        {
            return Ok(teamMemberRequest.Value);
        }

        _logger.Warning("Failed to retrieve team membership for {teamId} with errors {@errors}", Id, teamMemberRequest.Error);

        return BadRequest();
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
