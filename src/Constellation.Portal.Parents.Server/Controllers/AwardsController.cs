namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Application.Models.Identity;
using Constellation.Application.Awards.GetSummaryForStudent;
using Core.Errors;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.ActiveDirectory;

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
    public async Task<ApiResult<StudentAwardSummaryResponse>> GetDetails([FromRoute] string Id)
    {
        bool authorised = await HasAuthorizedAccessToStudent(_mediator, Id);

        if (!authorised)
            return ApiResult.FromResult(Result.Failure<StudentAwardSummaryResponse>(DomainErrors.Auth.NotAuthorised));

        AppUser? user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve award details for student {id} by parent {name}", Id, user.UserName);

        Result<StudentAwardSummaryResponse>? summary = await _mediator.Send(new GetSummaryForStudentQuery(Id));

        return ApiResult.FromResult(summary);
    }

    [HttpPost("Student/{studentId}/Download/{awardId}")]
    public async Task<IActionResult> GetAwardCertificate([FromRoute] string studentId, [FromRoute] string awardId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.LogInformation("Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

        bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

        if (!authorised)
        {
            _logger.LogWarning("UNAUTHORISED Requested to retrieve award certificate for student {id} by parent {name}", studentId, user.UserName);

            return Ok(ApiResult.FromResult(Result.Failure(DomainErrors.Auth.NotAuthorised)));
        }

        // Create file as stream
        Result<AttachmentResponse>? fileEntry = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.AwardCertificate, awardId));

        if (fileEntry.IsFailure)
        {
            _logger.LogWarning("Failed to retrieve file from database");

            return Ok(ApiResult.FromResult(fileEntry));
        }

        return File(new MemoryStream(fileEntry.Value.FileData), "application/pdf");
    }
}
