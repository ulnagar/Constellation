namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Reports;

using Application.DTOs;
using Application.Models.Auth;
using Application.Training.Modules.GenerateOverallReport;
using Application.Training.Modules.GenerateStaffReport;
using BaseModels;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Modules.GenerateModuleReport;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Pages.Shared.PartialViews.SelectStaffMemberForReportModal;
using Server.Pages.Shared.PartialViews.SelectTrainingModuleForReportModal;

[Authorize(Policy = AuthPolicies.CanRunTrainingModuleReports)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Reports;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

    }

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
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

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
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

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
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

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
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

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
            StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

            Error = new()
            {
                Error = new("Page.Validation", ModelState.First().Value.Errors.First().ErrorMessage),
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/Training/Reports/StaffMember", new { area = "SchoolAdmin", Id = viewModel.StaffId });
    }
}