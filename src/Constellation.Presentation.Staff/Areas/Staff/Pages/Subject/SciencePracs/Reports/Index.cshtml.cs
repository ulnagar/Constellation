namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Reports;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.GenerateOverdueReport;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Reports;
    [ViewData] public string PageTitle => "Lesson Roll Reports";

    public async Task OnGet() { }

    public async Task<IActionResult> OnGetDownloadReport()
    {
        _logger.Information("Requested to download Overdue Lesson report by user {User}", _currentUserService.UserName);

        Result<FileDto> reportRequest = await _mediator.Send(new GenerateOverdueReportCommand());

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to download Overdue Lesson report by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }
}