namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Reports.Academic;

using Application.Common.PresentationModels;
using Application.Domains.Attachments.Queries.GetAttachmentFile;
using Application.Domains.StudentReports.Queries.GetAllAcademicReports;
using Application.Models.Auth;
using Constellation.Core.Models.Attachments.DTOs;
using Constellation.Core.Models.Attachments.ValueObjects;
using Core.Abstractions.Services;
using Core.Models.Reports.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Reports_Academic;
    [ViewData] public string PageTitle => "Academic Report List";

    public List<AcademicReportSummary> Reports { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<AcademicReportSummary>> reports = await _mediator.Send(new GetAllAcademicReportsQuery());

        if (reports.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reports.Error, true)
                .Warning("Failed to retrieve Academic Reports by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                reports.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/Academic/Index", values: new { area = "Staff" }));

            return;
        }

        Reports = reports.Value;
    }

    public async Task<IActionResult> OnGetDownloadReport(AcademicReportId id)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.StudentReport, id.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                fileResponse.Error,
                _linkGenerator.GetPathByPage("/Reports/Index", values: new { area = "Parents" }));

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }
}
