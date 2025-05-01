namespace Constellation.Presentation.Students.Areas.Students.Pages.Reports;

using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Domains.StudentReports.Queries.GetCombinedReportListForStudent;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Reports.Identifiers;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStudent)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StudentPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Reports;

    public List<ReportResponse> Reports { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetDownloadAcademicReport(AcademicReportId reportId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, reportId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Reports/Index", values: new { area = "Students" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadExternalReport(ExternalReportId reportId)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.ExternalReport, reportId.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Reports/Index", values: new { area = "Students" }));

            await PreparePage();

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }

    public async Task PreparePage()
    {
        _logger.Information("Requested to retrieve reports by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve reports by user {user}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<List<ReportResponse>> reportsRequest = await _mediator.Send(new GetCombinedReportListForStudentQuery(studentId));

        if (reportsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(reportsRequest.Error);

            return;
        }

        Reports = reportsRequest.Value;
    }
}