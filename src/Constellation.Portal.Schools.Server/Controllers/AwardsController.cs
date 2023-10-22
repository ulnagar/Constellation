namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Constellation.Application.Awards.GetSummaryForStudent;
using Core.Models.Attachments.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class AwardsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public AwardsController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<AwardsController>();
    }

    [HttpGet("ForStudent/{studentId}")]
    public async Task<StudentAwardSummaryResponse> GetForSchool(string studentId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to get awards for student {studentId} by user {user}", studentId, user.DisplayName);

        var awards = await _mediator.Send(new GetSummaryForStudentQuery(studentId));

        return awards.Value;
    }

    [HttpPost("ForStudent/{studentId}/Download/{awardId}")]
    public async Task<IActionResult> GetAwardCertificate([FromRoute] string studentId, [FromRoute] string awardId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

        // Create file as stream
        var fileEntry = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId));

        if (fileEntry.IsFailure)
        {
            return BadRequest();
        }

        return File(new MemoryStream(fileEntry.Value.FileData), "application/pdf");
    }
}
