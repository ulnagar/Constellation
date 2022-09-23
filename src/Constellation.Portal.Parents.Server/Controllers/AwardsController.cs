namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Features.Awards.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class AwardsController : BaseAPIController
{
    private readonly ILogger<AwardsController> _logger;
    private readonly IMediator _mediator;

    public AwardsController(ILogger<AwardsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("Student/{id}")]
    public async Task<StudentAwardSummary> GetDetails([FromRoute] string Id)
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve award details for student {id} by parent {name}", Id, user.UserName);

        var summary = await _mediator.Send(new GetAwardSummaryForStudentQuery { StudentId = Id });

        return summary;
    }
}
