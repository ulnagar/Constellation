namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Features.Documents.Queries;
using Constellation.Application.Features.Portal.School.Reports.Queries;
using Constellation.Core.Models;
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
	public async Task<ICollection<StudentReportForList>> GetReportsList([FromRoute] string studentId)
	{
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
		{
			return new List<StudentReportForList>();
		}

		var user = await GetCurrentUser();

		_logger.LogInformation("Requested to retrieve contacts for student {studentId} by user {user}", studentId, user.UserName);

		return await _mediator.Send(new GetStudentReportListQuery { StudentId = studentId });
	}

    [HttpPost("Student/{studentId}/Download/{entryId}")]
    public async Task<IActionResult> GetAcademicReport([FromRoute] string entryId, [FromRoute] string studentId)
    {
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
			return BadRequest();

        // Create file as stream
        var fileEntry = await _mediator.Send(new GetFileFromDatabaseQuery { LinkType = StoredFile.StudentReport, LinkId = entryId });

        var filename = $"Academic Report - {DateTime.Today:yyyy-MM-dd}.pdf";

		return File(new MemoryStream(fileEntry.FileData), "application/pdf", filename);
    }
}
