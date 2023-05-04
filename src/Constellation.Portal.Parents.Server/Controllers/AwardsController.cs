namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Awards.GetSummaryForStudent;
using Constellation.Application.Features.Documents.Queries;
using Constellation.Application.Features.Portal.School.Timetables.Queries;
using Constellation.Core.Models;
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

    [HttpPost("Student/{studentId}/Download/{awardId}")]
    public async Task<IActionResult> GetAwardCertificate([FromRoute] string studentId, [FromRoute] string awardId)
    {
        var user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

        var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

        if (!authorised)
        {
            _logger.LogWarning("UNAUTHORISED Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

            return BadRequest();
        }

        // Create file as stream
        var fileEntry = await _mediator.Send(new GetFileFromDatabaseQuery { LinkType = StoredFile.AwardCertificate, LinkId = awardId });

        return File(new MemoryStream(fileEntry.FileData), "application/pdf");
    }
}
