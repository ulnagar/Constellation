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
    public async Task<ApiResult<List<ScienceLessonRollSummary>>> GetForSchool([FromRoute] string code)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Science Lesson Rolls for school {code} by user {user}", code, user.DisplayName);

        Result<List<ScienceLessonRollSummary>>? rollsRequest = await _mediator.Send(new GetLessonRollsForSchoolQuery(code));

        return ApiResult.FromResult(rollsRequest);
    }

    [HttpGet("Details/{rollId:guid}")]
    public async Task<ApiResult<ScienceLessonRollDetails>> GetRollDetails([FromRoute] Guid rollId, Guid lessonId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll details for roll {code} by user {user}", rollId, user.DisplayName);

        SciencePracLessonId LessonId = SciencePracLessonId.FromValue(lessonId);
        SciencePracRollId RollId = SciencePracRollId.FromValue(rollId);

        Result<ScienceLessonRollDetails>? request = await _mediator.Send(new GetLessonRollDetailsForSchoolsPortalQuery(LessonId, RollId));

        return ApiResult.FromResult(request);
    }

    [HttpGet("ForSubmit/{rollId:guid}")]
    public async Task<ApiResult<ScienceLessonRollForSubmit>> GetRollForSubmit([FromRoute] Guid rollId, Guid lessonId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Roll submission details for roll {code} by user {user}", rollId, user.DisplayName);

        SciencePracLessonId LessonId = SciencePracLessonId.FromValue(lessonId);
        SciencePracRollId RollId = SciencePracRollId.FromValue(rollId);

        Result<ScienceLessonRollForSubmit>? request = await _mediator.Send(new GetLessonRollSubmitContextForSchoolsPortalQuery(LessonId, RollId));

        return ApiResult.FromResult(request);
    }

    [HttpPost("Submit/{rollId:guid}")]
    public async Task<ApiResult> SubmitMarkedRoll(
        [FromRoute] Guid rollId, 
        [FromBody] SubmitRollCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to submit roll with details {@details} by user {user}", command, user.DisplayName);

        Result? response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }
}
