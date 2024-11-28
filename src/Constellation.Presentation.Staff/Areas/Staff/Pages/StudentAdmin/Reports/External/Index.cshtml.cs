namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Reports.External;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Reports.GetExternalReportList;
using Constellation.Application.Attachments.GetAttachmentFile;
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
using Serilog;

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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData]
    public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Reports_External;

    [ViewData] 
    public string PageTitle => "External Reports List";

    public List<ExternalReportResponse> Reports { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve External Reports by user {User}", _currentUserService.UserName);

        Result<List<ExternalReportResponse>> reports = await _mediator.Send(new GetExternalReportListQuery());

        if (reports.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reports.Error, true)
                .Warning("Failed to retrieve External Reports by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                reports.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Reports/External/Index", values: new { area = "Staff" }));

            return;
        }

        Reports = reports.Value;
    }

    public async Task<IActionResult> OnGetDownload(
        ExternalReportId id)
    {
        Result<AttachmentResponse> fileResponse = await _mediator.Send(new GetAttachmentFileQuery(AttachmentType.ExternalReport, id.ToString()));

        if (fileResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(fileResponse.Error, null);

            return Page();
        }

        return File(fileResponse.Value.FileData, fileResponse.Value.FileType, fileResponse.Value.FileName);
    }
}
