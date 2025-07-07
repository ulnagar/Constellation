namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Queries.GetStaffMembersAsDictionary;
using Application.Domains.Training.Models;
using Application.Domains.Training.Queries.GenerateModuleReport;
using Application.Domains.Training.Queries.GenerateOverallReport;
using Application.Domains.Training.Queries.GenerateStaffReport;
using Application.Domains.Training.Queries.GetTrainingModulesAsDictionary;
using Application.DTOs;
using Application.Models.Auth;
using Areas;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        Dictionary<Guid, string> moduleList = new();

        Result<Dictionary<Guid, string>> moduleListRequest = await _mediator.Send(new GetTrainingModulesAsDictionaryQuery());

        if (moduleListRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), moduleListRequest.Error, true)
                .Warning("Failed to initialise Training Completion upload page by user {User}", _currentUserService.UserName);
        }
        else
        {
            moduleList = moduleListRequest.Value;
        }

        SelectTrainingModuleForReportModalViewModel viewModel = new()
        {
            DetailedReportRequested = detailsRequested,
            Modules = moduleList
        };

        return Partial("SelectTrainingModuleForReportModal", viewModel);
    }

    public async Task<IActionResult> OnPostModuleSummaryReport(SelectTrainingModuleForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

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
            
            ModalContent = ErrorDisplay.Create(reportRequest.Error);

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

            ModalContent = ErrorDisplay.Create(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostModuleDetailReport(SelectTrainingModuleForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

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

            ModalContent = ErrorDisplay.Create(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostAjaxStaffModal(string reportType)
    {
        Dictionary<StaffId, string> staffList = new();

        Result<Dictionary<StaffId, string>> staffListRequest = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

        if (staffListRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffListRequest.Error, true)
                .Warning("Failed to retrieve list of staff by user {User}", _currentUserService.UserName);
        }
        else
        {
            staffList = staffListRequest.Value;
        }
        
        SelectStaffMemberForReportModalViewModel.ReportType type = reportType == "summary" ? SelectStaffMemberForReportModalViewModel.ReportType.Summary
            : reportType == "detail" ? SelectStaffMemberForReportModalViewModel.ReportType.Detail
            : SelectStaffMemberForReportModalViewModel.ReportType.Module;

        SelectStaffMemberForReportModalViewModel viewModel = new()
        {
            Type = type,
            StaffMembers = staffList
        };

        return Partial("SelectStaffMemberForReportModal", viewModel);
    }

    public async Task<IActionResult> OnPostStaffSummaryReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        _logger.Information("Requested to generate staff Training report by user {User}", _currentUserService.UserName);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, false));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate staff Training report by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffDetailReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        _logger.Information("Requested to generate staff Training detail report by user {User}", _currentUserService.UserName);
        
        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, true));

        if (reportRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), reportRequest.Error, true)
                .Warning("Failed to generate staff Training detail report by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(reportRequest.Error);

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffModuleReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = ErrorDisplay.Create(new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage));

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Reports/StaffMember", new { area = "Staff", Id = viewModel.StaffId });
    }
}