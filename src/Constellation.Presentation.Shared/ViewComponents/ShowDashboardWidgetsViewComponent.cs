namespace Constellation.Presentation.Shared.ViewComponents;

using Application.Training.CountStaffWithoutModule;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.CountStudentsWithAbsenceScanDisabled;
using Constellation.Application.Students.CountStudentsWithAwardOverages;
using Constellation.Application.Students.CountStudentsWithoutSentralId;
using Constellation.Application.Students.CountStudentsWithPendingAwards;
using Constellation.Application.WorkFlows.CountActiveActionsForUser;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.ShowDashboardWidgets;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ShowDashboardWidgetsViewComponent : ViewComponent
{
    private readonly ISender _mediator;

    public ShowDashboardWidgetsViewComponent(
        ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        bool isStaff = User.IsInRole(AuthRoles.StaffMember);
        bool isAdmin = User.IsInRole(AuthRoles.Admin);
        bool isTrainingManager = User.IsInRole(AuthRoles.MandatoryTrainingEditor);
        bool isAbsencesManager = User.IsInRole(AuthRoles.AbsencesEditor);
        bool isAwardsManager = User.IsInRole(AuthRoles.AwardsManager);

        string staffId = user.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value ?? string.Empty;

        ShowDashboardWidgetsViewComponentModel viewModel = new();

        if (!string.IsNullOrWhiteSpace(staffId))
        {
            Result<int> countOfActiveActions = await _mediator.Send(new CountActiveActionsForUserQuery(staffId), cancellationToken);
            if (countOfActiveActions.IsSuccess)
                viewModel.ActiveWorkFlowActions = countOfActiveActions.Value;
        }

        if (isTrainingManager || isAdmin)
        {
            viewModel.ShowTrainingWidgets = true;

            Result<int> countOfStaffWithoutRoles = await _mediator.Send(new CountStaffWithoutModuleQuery(), cancellationToken);
            if (countOfStaffWithoutRoles.IsSuccess)
                viewModel.WithoutRole = countOfStaffWithoutRoles.Value;
        }

        if (isAbsencesManager || isAdmin)
        {
            viewModel.ShowAbsenceWidgets = true;

            Result<(int Whole, int Partial)> absenceScanRequest = await _mediator.Send(new CountStudentsWithAbsenceScanDisabledQuery(), cancellationToken);

            if (absenceScanRequest.IsSuccess)
            {
                viewModel.WholeScanDisabled = absenceScanRequest.Value.Whole;
                viewModel.PartialScanDisabled = absenceScanRequest.Value.Partial;
            }
        }

        if (isAwardsManager || isAdmin)
        {
            viewModel.ShowAwardsWidgets = true;

            Result<int> overages = await _mediator.Send(new CountStudentsWithAwardOveragesQuery(), cancellationToken);

            if (overages.IsSuccess)
            {
                viewModel.AwardOverages = overages.Value;
            }

            Result<int> pending = await _mediator.Send(new CountStudentsWithPendingAwardsQuery(), cancellationToken);

            if (pending.IsSuccess)
            {
                viewModel.AwardAdditions = pending.Value;
            }
        }

        if (isAdmin)
        {
            viewModel.ShowSentralIdWidgets = true;

            Result<int> sentralIdRequest = await _mediator.Send(new CountStudentsWithoutSentralIdQuery(), cancellationToken);

            if (sentralIdRequest.IsSuccess)
            {
                viewModel.StudentsWithoutSentralId = sentralIdRequest.Value;
            }
        }

        return View(viewModel);
    }
}