namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Views.Shared.PartialViews.SelectStaffMemberForReportModal;
using Views.Shared.PartialViews.SelectTrainingModuleForReportModal;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
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
            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, false));

        if (reportRequest.IsFailure)
        {
            Error = new()
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnGetOverviewReport()
    {
        Result<FileDto> reportRequest = await _mediator.Send(new GenerateOverviewReportCommand());

        if (reportRequest.IsFailure)
        {
            Error = new()
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostModuleDetailReport(SelectTrainingModuleForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, true));

        if (reportRequest.IsFailure)
        {
            Error = new()
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

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
            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, false));

        if (reportRequest.IsFailure)
        {
            Error = new()
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffDetailReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateStaffReportCommand(viewModel.StaffId, true));

        if (reportRequest.IsFailure)
        {
            Error = new()
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostStaffModuleReport(SelectStaffMemberForReportModalViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Training/Reports/StaffMember", new { area = "Staff", Id = viewModel.StaffId });
    }
}