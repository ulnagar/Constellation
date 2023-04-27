namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Awards.GetSummaryForStudent;
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
    public async Task<StudentAwardSummaryResponse> GetDetails([FromRoute] string Id)
    {
        var authorised = await HasAuthorizedAccessToStudent(_mediator, Id);

        if (!authorised)
            return new StudentAwardSummaryResponse(0, 0, 0, 0, new());

        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve award details for student {id} by parent {name}", Id, user.UserName);

        var summary = await _mediator.Send(new GetSummaryForStudentQuery(Id));

        return summary.Value;
    }
}
