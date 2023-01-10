using Constellation.Application.Features.Portal.School.ScienceRolls.Commands;
using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using Constellation.Application.Features.Portal.School.ScienceRolls.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Portal.Schools.Server.Controllers;

[Route("api/[controller]")]
public class RollsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public RollsController(IMediator mediator, Serilog.ILogger logger)
	{
        _mediator = mediator;
        _logger = logger.ForContext<RollsController>();
    }

    [HttpGet("ForSchool/{code}")]
    public async Task<List<ScienceLessonRollForList>> GetForSchool([FromRoute] string code)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Science Lesson Rolls for school {code} by user {user}", code, user.DisplayName);

        var rolls = await _mediator.Send(new GetScienceLessonRollsForSchoolQuery { SchoolCode = code });

        return rolls.ToList();
    }

    [HttpGet("Details/{rollId:guid}")]
    public async Task<ScienceLessonRollForDetails> GetRollDetails([FromRoute] Guid rollId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll details for roll {code} by user {user}", rollId, user.DisplayName);

        return await _mediator.Send(new GetScienceLessonRollForDisplayQuery { RollId = rollId });
    }

    [HttpGet("ForSubmit/{rollId:guid}")]
    public async Task<ScienceLessonRollForSubmit> GetRollForSubmit([FromRoute] Guid rollId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll submission details for roll {code} by user {user}", rollId, user.DisplayName);

        return await _mediator.Send(new GetScienceLessonRollForSubmitQuery { RollId = rollId });
    }

    [HttpPost("Submit/{rollId:guid}")]
    public async Task SubmitMarkedRoll([FromRoute] Guid rollId, [FromBody] SubmitScienceLessonRollCommand command)
    {
        var user = await GetCurrentUser();

        command.UserEmail = user.Email;

        _logger.Information("Requested to submit roll with details {@details} by user {user}", command, user.DisplayName);

        await _mediator.Send(command);
    }
}
