﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardWidgets;

using Application.Domains.Attendance.Plans.Queries.CountAttendancePlansWithStatus;
using Application.Domains.Edval.Queries.CountEdvalDifferences;
using Application.Domains.Students.Queries.CountStudentsWithAbsenceScanDisabled;
using Application.Domains.Students.Queries.CountStudentsWithAwardOverages;
using Application.Domains.Students.Queries.CountStudentsWithoutSentralId;
using Application.Domains.Students.Queries.CountStudentsWithPendingAwards;
using Application.Domains.Training.Queries.CountStaffWithoutModule;
using Application.Domains.WorkFlows.Queries.CountActiveActionsForUser;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ShowDashboardWidgetsViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _mediator;

    public ShowDashboardWidgetsViewComponent(
        ICurrentUserService currentUserService,
        ISender mediator)
    {
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        //TODO: 1.17.1: Update to AuthorizationService checks against AuthPolicies instead of group memberships
        bool isStaff = User.IsInRole(AuthRoles.StaffMember);
        bool isAdmin = User.IsInRole(AuthRoles.Admin);
        bool isTrainingManager = User.IsInRole(AuthRoles.MandatoryTrainingEditor);
        bool isAbsencesManager = User.IsInRole(AuthRoles.AbsencesEditor);
        bool isAwardsManager = User.IsInRole(AuthRoles.AwardsManager);

        StaffId staffId = _currentUserService.StaffId;

        ShowDashboardWidgetsViewComponentModel viewModel = new();

        if (staffId != StaffId.Empty)
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

            Result<(int Pending, int Processing)> attendancePlanRequest = await _mediator.Send(new CountAttendancePlansWithStatusQuery(), cancellationToken);

            if (attendancePlanRequest.IsSuccess)
            {
                viewModel.PendingAttendancePlans = attendancePlanRequest.Value.Pending;
                viewModel.ProcessingAttendancePlans = attendancePlanRequest.Value.Processing;
            }

            Result<(int Active, int Ignored)> edvalDifferencesRequest = await _mediator.Send(new CountEdvalDifferencesQuery(), cancellationToken);

            if (edvalDifferencesRequest.IsSuccess)
            {
                viewModel.EdvalDifferences = edvalDifferencesRequest.Value.Active;
            }
        }

        if (isAwardsManager || isAdmin)
        {
            viewModel.ShowAwardsWidgets = true;

            Result<int> overages = await _mediator.Send(new CountStudentsWithAwardOveragesQuery(), cancellationToken);

            if (overages.IsSuccess)
                viewModel.AwardOverages = overages.Value;

            Result<int> pending = await _mediator.Send(new CountStudentsWithPendingAwardsQuery(), cancellationToken);

            if (pending.IsSuccess)
                viewModel.AwardAdditions = pending.Value;
        }

        if (isAdmin)
        {
            viewModel.ShowSentralIdWidgets = true;

            Result<int> sentralIdRequest = await _mediator.Send(new CountStudentsWithoutSentralIdQuery(), cancellationToken);

            if (sentralIdRequest.IsSuccess)
                viewModel.StudentsWithoutSentralId = sentralIdRequest.Value;
        }

        return View(viewModel);
    }
}