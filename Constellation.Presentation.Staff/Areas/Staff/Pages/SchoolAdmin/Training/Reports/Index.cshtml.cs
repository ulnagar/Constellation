namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Training.Reports;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.DTOs;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Modules.GenerateModuleReport;
using Constellation.Application.Training.Modules.GenerateOverallReport;
using Constellation.Application.Training.Modules.GenerateStaffReport;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Pages.Shared.PartialViews.SelectStaffMemberForReportModal;
using Presentation.Shared.Pages.Shared.PartialViews.SelectTrainingModuleForReportModal;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Training_Reports;

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
            Error = new ErrorDisplay
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
            Error = new ErrorDisplay
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
            Error = new ErrorDisplay
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
            Error = new ErrorDisplay
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
            Error = new ErrorDisplay
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