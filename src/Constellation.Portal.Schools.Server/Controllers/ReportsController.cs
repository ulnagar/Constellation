namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Portal.School.Reports.Models;
using Constellation.Application.Features.Portal.School.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class ReportsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public ReportsController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<ReportsController>();
    }

    [HttpGet("ForSchool/{code}")]
    public async Task<List<StudentReportForDownload>> GetForSchool([FromRoute] string code)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Student Reports for school {code} by user {user}", code, user.DisplayName);

        var reports = await _mediator.Send(new GetStudentReportListForSchoolQuery { SchoolCode = code });

        return reports.ToList();
    }

    [HttpPost("Download")]
    public async Task<IActionResult> DownloadReport([FromBody] Guid reportId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to download Student Report with id {reportId} by user {user}", reportId.ToString(), user.DisplayName);

        var file = await _mediator.Send(new GetFileFromDatabaseQuery { LinkType = Core.Models.StoredFile.StudentReport, LinkId = reportId.ToString() });

        return File(file.FileData, file.FileType, file.Name);
    }
}
