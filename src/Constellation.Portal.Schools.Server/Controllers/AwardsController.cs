namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Application.Models.Identity;
using Constellation.Application.Awards.GetSummaryForStudent;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

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
    public async Task<ApiResult<StudentAwardSummaryResponse>> GetForSchool(string studentId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to get awards for student {studentId} by user {user}", studentId, user.DisplayName);

        Result<StudentAwardSummaryResponse>? awards = await _mediator.Send(new GetSummaryForStudentQuery(studentId));

        return ApiResult.FromResult(awards);
    }

    [HttpPost("ForStudent/{studentId}/Download/{awardId}")]
    public async Task<IActionResult> GetAwardCertificate([FromRoute] string studentId, [FromRoute] string awardId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

        // Create file as stream
        Result<AttachmentResponse>? fileEntry = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId));

        if (fileEntry.IsFailure)
            return Ok(ApiResult.FromResult(fileEntry));

        return File(new MemoryStream(fileEntry.Value.FileData), MediaTypeNames.Application.Pdf);
    }
}
