namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Reports;

using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Training.Models;
using Constellation.Application.Training.Modules.GenerateModuleReport;
using Constellation.Core.Models.Training.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        await GetClasses(_mediator);


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

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, false));

        if (reportRequest.IsFailure)
        {
            await GetClasses(_mediator);

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

        TrainingModuleId moduleId = TrainingModuleId.FromValue(viewModel.ModuleId);

        Result<ReportDto> reportRequest = await _mediator.Send(new GenerateModuleReportCommand(moduleId, true));

        if (reportRequest.IsFailure)
        {
            await GetClasses(_mediator);

            Error = new ErrorDisplay
            {
                Error = reportRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(reportRequest.Value.FileData, reportRequest.Value.FileType, reportRequest.Value.FileName);
    }
}