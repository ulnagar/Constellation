namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Reports;

using Application.Common.PresentationModels;
using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;
using Application.Models.Auth;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Reports.Identifiers;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Models.Attachments.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Reports_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger) 
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Reports;

    public List<SchoolReportResponse> Reports { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve report data by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<SchoolReportResponse>> reportsResponse = await _mediator.Send(new GetCombinedReportListForSchoolQuery(CurrentSchoolCode));
        
        if (reportsResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(reportsResponse.Error);

            return;
        }

        Reports = reportsResponse.Value
            .OrderBy(report => report.Grade)
            .ThenBy(report => report.LastName)
            .ThenBy(report => report.FirstName)
            .ToList();
    }

    public async Task<IActionResult> OnGetDownload(AcademicReportId reportId)
    {
        _logger.Information("Requested to download report data by user {user} for Id {reportId}", _currentUserService.UserName, reportId);

        Result<AttachmentResponse> file = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, reportId.ToString()));
        
        if (file.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(file.Error);

            _logger.Information("Requested to retrieve report data by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

            Result<List<SchoolReportResponse>> reportsResponse = await _mediator.Send(new GetCombinedReportListForSchoolQuery(CurrentSchoolCode));

            Reports = reportsResponse.Value
                .OrderBy(report => report.Grade)
                .ThenBy(report => report.LastName)
                .ThenBy(report => report.FirstName)
                .ToList();

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }

    public async Task<IActionResult> OnGetDownloadExternal(ExternalReportId reportId)
    {
        _logger.Information("Requested to download report data by user {user} for Id {reportId}", _currentUserService.UserName, reportId);

        Result<AttachmentResponse> file = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.ExternalReport, reportId.ToString()));

        if (file.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(file.Error);

            _logger.Information("Requested to retrieve report data by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

            Result<List<SchoolReportResponse>> reportsResponse = await _mediator.Send(new GetCombinedReportListForSchoolQuery(CurrentSchoolCode));

            Reports = reportsResponse.Value
                .OrderBy(report => report.Grade)
                .ThenBy(report => report.LastName)
                .ThenBy(report => report.FirstName)
                .ToList();

            return Page();
        }

        return File(file.Value.FileData, file.Value.FileType, file.Value.FileName);
    }
}