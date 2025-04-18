namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

using Application.Common.PresentationModels;
using Application.DTOs;
using Application.Features.Common.Queries;
using Application.Models.Auth;
using Application.Training.GenerateModuleReport;
using Application.Training.GenerateOverallReport;
using Application.Training.GenerateStaffReport;
using Areas;
using Constellation.Application.Training.Models;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using Shared.PartialViews.SelectStaffMemberForReportModal;
using Shared.PartialViews.SelectTrainingModuleForReportModal;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Reports;
    [ViewData] public string PageTitle => "Training Module Reports";

    public async Task OnGet() { }

    public async Task<IActionResult> OnPostAjaxModuleModal(bool detailsRequested)
    {
        Dictionary<Guid, string> modules = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());
        
        SelectTrainingModuleForReportModalViewModel viewModel = new()
        {
            DetailedReportRequested = detailsRequested,
            Modules = modules
        };

        return Partial("SelectTrainingModuleForReportModal", viewModel);
    }

    public async Task<IActionResult> OnPostModuleSummaryReport(SelectTrainingModuleForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new ErrorDisplay(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        _logger.Information("Requested to generate Training Module report by user {User}", _currentUserService.UserName);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, false));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate Training Module report by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetOverviewReport()
    {
        _logger.Information("Requested to generate Training overview report by user {User}", _currentUserService.UserName);

        Result<FileDto> reportRequest = await _mediator.Send(new GenerateOverviewReportCommand());

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Requested to generate Training overview report by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostModuleDetailReport(SelectTrainingModuleForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new ErrorDisplay(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        _logger.Information("Requested to generate Training Module detail report by user {User}", _currentUserService.UserName);
        
        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, true));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate Training Module detail report by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostAjaxStaffModal(string reportType)
    {
        Dictionary<string, string> staffResult = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        SelectStaffMemberForReportModalViewModel.ReportType type = reportType == "summary" ? SelectStaffMemberForReportModalViewModel.ReportType.Summary
            : reportType == "detail" ? SelectStaffMemberForReportModalViewModel.ReportType.Detail
            : SelectStaffMemberForReportModalViewModel.ReportType.Module;

        SelectStaffMemberForReportModalViewModel viewModel = new()
        {
            Type = type,
            StaffMembers = staffResult
        };

        return Partial("SelectStaffMemberForReportModal", viewModel);
    }

    public async Task<IActionResult> OnPostStaffSummaryReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new ErrorDisplay(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        _logger.Information("Requested to generate staff Training report by user {User}", _currentUserService.UserName);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, false));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate staff Training report by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffDetailReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new ErrorDisplay(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        _logger.Information("Requested to generate staff Training detail report by user {User}", _currentUserService.UserName);
        
        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, true));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate staff Training detail report by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffModuleReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new ErrorDisplay(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Reports/StaffMember", new { area = "Staff", Id = viewModel.StaffId });
    }
}