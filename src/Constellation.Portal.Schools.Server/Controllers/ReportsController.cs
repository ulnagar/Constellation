namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Attachments.GetAttachmentFile;
using Application.Models.Identity;
using Constellation.Application.Reports.GetStudentReportsForSchool;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Shared;
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
    public async Task<ApiResult<List<SchoolStudentReportResponse>>> GetForSchool([FromRoute] string code)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve Student Reports for school {code} by user {user}", code, user.DisplayName);

        Result<List<SchoolStudentReportResponse>>? reports = await _mediator.Send(new GetStudentReportsForSchoolQuery(code));
        
        return ApiResult.FromResult(reports);
    }

    [HttpPost("Download")]
    public async Task<IActionResult> DownloadReport([FromBody] Guid reportId)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to download Student Report with id {reportId} by user {user}", reportId.ToString(), user.DisplayName);

        Result<AttachmentResponse>? file = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, reportId.ToString()));

        if (file.IsFailure)
            return Ok(ApiResult.FromResult(file));

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }
}
