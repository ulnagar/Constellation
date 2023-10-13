namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Constellation.Application.Reports.GetAcademicReportList;
using Core.Models.Attachments.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class ReportsController : BaseAPIController
{
	private readonly ILogger<ReportsController> _logger;
	private readonly IMediator _mediator;

	public ReportsController(ILogger<ReportsController> logger, IMediator mediator)
	{
		_logger = logger;
		_mediator = mediator;
	}

	[HttpGet("Student/{studentId}")]
	public async Task<List<AcademicReportResponse>> GetReportsList([FromRoute] string studentId)
	{
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
		{
			return new List<AcademicReportResponse>();
		}

		var user = await GetCurrentUser();

		_logger.LogInformation("Requested to retrieve reports for student {studentId} by user {user}", studentId, user.UserName);

		var result = await _mediator.Send(new GetAcademicReportListQuery(studentId));

		if (result.IsSuccess)
		{
			return result.Value;
		}

		return new List<AcademicReportResponse>();
	}

    [HttpPost("Student/{studentId}/Download/{entryId}")]
    public async Task<IActionResult> GetAcademicReport([FromRoute] string entryId, [FromRoute] string studentId)
    {
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
			return BadRequest();

        // Create file as stream
        var fileEntry = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, entryId));

        if (fileEntry.IsFailure)
        {
            return BadRequest();
        }

        var filename = $"Academic Report - {DateTime.Today:yyyy-MM-dd}.pdf";

		return File(new MemoryStream(fileEntry.Value.FileData), "application/pdf", filename);
    }
}
