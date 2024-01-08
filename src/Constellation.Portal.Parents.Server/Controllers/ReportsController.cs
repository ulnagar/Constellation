namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Application.Models.Identity;
using Constellation.Application.Reports.GetAcademicReportList;
using Core.Errors;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

[Route("[controller]")]
public class ReportsController : BaseAPIController
{
	private readonly Serilog.ILogger _logger;
	private readonly IMediator _mediator;

	public ReportsController(
        Serilog.ILogger logger, 
        IMediator mediator)
	{
		_logger = logger.ForContext<ReportsController>();
		_mediator = mediator;
	}

	[HttpGet("Student/{studentId}")]
	public async Task<ApiResult<List<AcademicReportResponse>>> GetReportsList([FromRoute] string studentId)
	{
		bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
            return ApiResult.FromResult(Result.Failure<List<AcademicReportResponse>>(DomainErrors.Auth.NotAuthorised));

		AppUser? user = await GetCurrentUser();

		_logger
            .Information("Requested to retrieve reports for student {studentId} by user {user}", studentId, user.UserName);

		Result<List<AcademicReportResponse>>? result = await _mediator.Send(new GetAcademicReportListQuery(studentId));

        return ApiResult.FromResult(result);
    }

    [HttpPost("Student/{studentId}/Download/{entryId}")]
    public async Task<IActionResult> GetAcademicReport([FromRoute] string entryId, [FromRoute] string studentId)
    {
		bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

        if (!authorised)
            return Ok(ApiResult.FromResult(Result.Failure(DomainErrors.Auth.NotAuthorised)));

        // Create file as stream
        Result<AttachmentResponse>? fileEntry = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, entryId));

        if (fileEntry.IsFailure)
        {
            return BadRequest();
        }

        string filename = $"Academic Report - {DateTime.Today:yyyy-MM-dd}.pdf";

		return File(new MemoryStream(fileEntry.Value.FileData), MediaTypeNames.Application.Pdf, filename);
    }
}
