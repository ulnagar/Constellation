namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardWidgets;

using Application.Domains.AssetManagement.Stocktake.Queries.CountStocktakeItemsOutstanding;
using Application.Domains.Attendance.Plans.Queries.CountAttendancePlansWithStatus;
using Application.Domains.Edval.Queries.CountEdvalDifferences;
using Application.Domains.Students.Queries.CountStudentsWithAbsenceScanDisabled;
using Application.Domains.Students.Queries.CountStudentsWithAwardOverages;
using Application.Domains.Students.Queries.CountStudentsWithoutSentralId;
using Application.Domains.Students.Queries.CountStudentsWithPendingAwards;
using Application.Domains.Training.Queries.CountStaffWithoutModule;
using Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingApproval;
using Application.Domains.Tutorials.Requests.Queries.CountRequestsPendingScheduling;
using Application.Domains.WorkFlows.Queries.CountActiveActionsForUser;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Stocktake.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class ShowDashboardWidgetsViewComponent : ViewComponent
{
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISender _mediator;

    public ShowDashboardWidgetsViewComponent(
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ISender mediator)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<IViewComponentResult> InvokeAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var adminTest = await _authService.AuthorizeAsync(user, AuthPolicies.IsSiteAdmin);
        var trainingTest = await _authService.AuthorizeAsync(user, AuthPermission.SchoolAdmin_Training_Edit_Value);
        var absencesTest = await _authService.AuthorizeAsync(user, AuthPermission.StudentAdmin_AttendanceSettings_Edit_Value);
        var awardsTest = await _authService.AuthorizeAsync(user, AuthPermission.StudentAdmin_Awards_Edit_Value);
        var tutorialsTest = await _authService.AuthorizeAsync(user, AuthPermission.Subjects_Tutorials_Edit_Value);

        StaffId staffId = _currentUserService.StaffId;

        ShowDashboardWidgetsViewComponentModel viewModel = new();

        if (staffId != StaffId.Empty)
        {
            Result<int> countOfActiveActions = await _mediator.Send(new CountActiveActionsForUserQuery(staffId), cancellationToken);
            if (countOfActiveActions.IsSuccess)
                viewModel.ActiveWorkFlowActions = countOfActiveActions.Value;
        }

        if (trainingTest.Succeeded || adminTest.Succeeded)
        {
            viewModel.ShowTrainingWidgets = true;

            Result<int> countOfStaffWithoutRoles = await _mediator.Send(new CountStaffWithoutModuleQuery(), cancellationToken);
            if (countOfStaffWithoutRoles.IsSuccess)
                viewModel.WithoutRole = countOfStaffWithoutRoles.Value;
        }

        if (absencesTest.Succeeded || adminTest.Succeeded)
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

        if (awardsTest.Succeeded || adminTest.Succeeded)
        {
            viewModel.ShowAwardsWidgets = true;

            Result<int> overages = await _mediator.Send(new CountStudentsWithAwardOveragesQuery(), cancellationToken);

            if (overages.IsSuccess)
                viewModel.AwardOverages = overages.Value;

            Result<int> pending = await _mediator.Send(new CountStudentsWithPendingAwardsQuery(), cancellationToken);

            if (pending.IsSuccess)
                viewModel.AwardAdditions = pending.Value;
        }

        if (tutorialsTest.Succeeded || adminTest.Succeeded)
        {
            viewModel.ShowTutorialRequestsWidget = true;

            Result<int> requestsForApproval = await _mediator.Send(new CountRequestsPendingApprovalQuery(), cancellationToken);

            if (requestsForApproval.IsSuccess)
                viewModel.TutorialRequestsPendingApproval = requestsForApproval.Value;

            Result<int> requestsForScheduling = await _mediator.Send(new CountRequestsPendingSchedulingQuery(), cancellationToken);

            if (requestsForScheduling.IsSuccess)
                viewModel.TutorialRequestsPendingScheduling = requestsForScheduling.Value;
        }

        if (adminTest.Succeeded)
        {
            viewModel.ShowSentralIdWidgets = true;

            Result<int> sentralIdRequest = await _mediator.Send(new CountStudentsWithoutSentralIdQuery(), cancellationToken);

            if (sentralIdRequest.IsSuccess)
                viewModel.StudentsWithoutSentralId = sentralIdRequest.Value;

            Result<(StocktakeEventId EventId, double Percentage)> stocktakeRequest = await _mediator.Send(new CountStocktakeItemsOutstandingQuery(), cancellationToken);

            if (stocktakeRequest.IsSuccess)
            {
                viewModel.ShowStocktakeWidget = true;
                viewModel.StocktakePercentage = stocktakeRequest.Value.Percentage;
                viewModel.StocktakeEventId = stocktakeRequest.Value.EventId;
            }
        }

        return View(viewModel);
    }
}