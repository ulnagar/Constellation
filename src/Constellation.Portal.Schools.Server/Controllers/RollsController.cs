namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Constellation.Application.SciencePracs.GetLessonRollDetailsForSchoolsPortal;
using Constellation.Application.SciencePracs.GetLessonRollsForSchoolsPortal;
using Constellation.Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal;
using Constellation.Application.SciencePracs.SubmitRoll;
using Constellation.Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<List<ScienceLessonRollSummary>> GetForSchool([FromRoute] string code)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Science Lesson Rolls for school {code} by user {user}", code, user.DisplayName);

        Result<List<ScienceLessonRollSummary>>? rollsRequest = await _mediator.Send(new GetLessonRollsForSchoolQuery(code));

        if (rollsRequest.IsFailure)
        {
            return new List<ScienceLessonRollSummary>();
        }

        return rollsRequest.Value;
    }

    [HttpGet("Details/{rollId:guid}")]
    public async Task<ScienceLessonRollDetails> GetRollDetails([FromRoute] Guid rollId, Guid lessonId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll details for roll {code} by user {user}", rollId, user.DisplayName);

        SciencePracLessonId LessonId = SciencePracLessonId.FromValue(lessonId);
        SciencePracRollId RollId = SciencePracRollId.FromValue(rollId);

        Result<ScienceLessonRollDetails>? request = await _mediator.Send(new GetLessonRollDetailsForSchoolsPortalQuery(LessonId, RollId));
    
        if (request.IsFailure)
        {
            return null;
        }

        return request.Value;
    }

    [HttpGet("ForSubmit/{rollId:guid}")]
    public async Task<ScienceLessonRollForSubmit> GetRollForSubmit([FromRoute] Guid rollId, Guid lessonId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll submission details for roll {code} by user {user}", rollId, user.DisplayName);

        SciencePracLessonId LessonId = SciencePracLessonId.FromValue(lessonId);
        SciencePracRollId RollId = SciencePracRollId.FromValue(rollId);

        Result<ScienceLessonRollForSubmit>? request = await _mediator.Send(new GetLessonRollSubmitContextForSchoolsPortalQuery(LessonId, RollId));
    
        if (request.IsFailure)
        {
            return null;
        }

        return request.Value;
    }

    [HttpPost("Submit/{rollId:guid}")]
    public async Task SubmitMarkedRoll([FromRoute] Guid rollId, [FromBody] SubmitRollCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to submit roll with details {@details} by user {user}", command, user.DisplayName);

        await _mediator.Send(command);
    }
}
