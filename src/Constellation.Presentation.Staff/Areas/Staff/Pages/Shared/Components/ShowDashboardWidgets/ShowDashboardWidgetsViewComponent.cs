namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardWidgets;

using Application.Domains.AssetManagement.Stocktake.Queries.CountStocktakeItemsOutstanding;
using Application.Domains.Attendance.Plans.Queries.CountAttendancePlansWithStatus;
using Application.Domains.Edval.Queries.CountEdvalDifferences;
using Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInPendingAcceptanceStatus;
using Application.Domains.EnrolmentContext.Offers.Queries.CountOffersInReviewingResponseStatus;
using Application.Domains.EnrolmentContext.Offers.Queries.GetChartDataForEnrolmentStatus;
using Application.Domains.MeritAwards.Awards.Enums;
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
using Microsoft.AspNetCore.Routing;
using PartialViews.DashboardWidget;
using System.Globalization;
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
        List<DashboardWidgetModel> widgets = [];

        var adminTest = await _authService.AuthorizeAsync(user, AuthPolicies.IsSiteAdmin);
        var trainingTest = await _authService.AuthorizeAsync(user, AuthPermission.SchoolAdmin_Training_Edit_Value);
        var absencesTest = await _authService.AuthorizeAsync(user, AuthPermission.StudentAdmin_AttendanceSettings_Edit_Value);
        var attendancePlanTest = await _authService.AuthorizeAsync(user, AuthPermission.StudentAdmin_AttendancePlans_Edit_Value);
        var awardsTest = await _authService.AuthorizeAsync(user, AuthPermission.StudentAdmin_Awards_Edit_Value);
        var tutorialsTest = await _authService.AuthorizeAsync(user, AuthPermission.Subjects_Tutorials_Edit_Value);
        var enrolmentOfferReviewer = await _authService.AuthorizeAsync(user, AuthPermission.Partners_Enrolments_Offers_Reviewer_Value);
        var enrolmentOfferApprover = await _authService.AuthorizeAsync(user, AuthPermission.Partners_Enrolments_Offers_Approver_Value);

        StaffId staffId = _currentUserService.StaffId;


        if (staffId != StaffId.Empty)
        {
            Result<int> countOfActiveActions = await _mediator.Send(new CountActiveActionsForUserQuery(staffId), cancellationToken);
            if (countOfActiveActions.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "workflow-actions",
                    "WorkFlow Actions",
                    countOfActiveActions.Value,
                    "active assigned WorkFlow Actions",
                    "/SchoolAdmin/WorkFlows/Index"));
            }
        }

        if (trainingTest.Succeeded || adminTest.Succeeded)
        {
            Result<int> countOfStaffWithoutRoles = await _mediator.Send(new CountStaffWithoutModuleQuery(), cancellationToken);
            if (countOfStaffWithoutRoles.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "training-withoutrole",
                    "Mandatory Training",
                    countOfStaffWithoutRoles.Value,
                    "staff members without an assigned Training Role",
                    "/SchoolAdmin/Training/Staff/WithoutModule"));
            }
        }

        if (absencesTest.Succeeded || adminTest.Succeeded)
        {
            Result<(int Whole, int Partial)> absenceScanRequest = await _mediator.Send(new CountStudentsWithAbsenceScanDisabledQuery(), cancellationToken);

            if (absenceScanRequest.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "absences-partialdisabled",
                    "Absence Scanning",
                    absenceScanRequest.Value.Partial,
                    "students with Partial Absence Scanning disabled",
                    "/StudentAdmin/Attendance/Configuration"));

                widgets.Add(new CountWidgetModel(
                    "absences-wholedisabled",
                    "Absence Scanning",
                    absenceScanRequest.Value.Whole,
                    "students with Whole Absence Scanning disabled",
                    "/StudentAdmin/Attendance/Configuration"));
            }

            Result<(int Active, int Ignored)> edvalDifferencesRequest = await _mediator.Send(new CountEdvalDifferencesQuery(), cancellationToken);

            if (edvalDifferencesRequest.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "absences-edval-differences",
                    "Edval Differences",
                    edvalDifferencesRequest.Value.Active,
                    "differences between Edval data and Constellation",
                    "/Subject/Periods/Edval/Index"));
            }
        }

        if (attendancePlanTest.Succeeded || adminTest.Succeeded)
        {
            Result<(int Pending, int Processing)> attendancePlanRequest = await _mediator.Send(new CountAttendancePlansWithStatusQuery(), cancellationToken);

            if (attendancePlanRequest.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "absences-plans-processing",
                    "Attendance Plans",
                    attendancePlanRequest.Value.Processing,
                    "attendance plans to process",
                    "/StudentAdmin/Attendance/Plans/Index"));

                widgets.Add(new CountWidgetModel(
                    "absences-plans-pending",
                    "Attendance Plans",
                    attendancePlanRequest.Value.Pending,
                    "attendance plans awaiting ACC entry",
                    "/StudentAdmin/Attendance/Plans/Index"));
            }
        }

        if (awardsTest.Succeeded || adminTest.Succeeded)
        {
            Result<int> overages = await _mediator.Send(new CountStudentsWithAwardOveragesQuery(), cancellationToken);

            if (overages.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "awards-overage",
                    "Awards",
                    overages.Value,
                    "students with Award overages",
                    "/StudentAdmin/Awards/Changes",
                    RouteValues: new Dictionary<string, string> { ["Filter"] = AwardsFilter.Overages.ToString() }));
            }

            Result<int> pending = await _mediator.Send(new CountStudentsWithPendingAwardsQuery(), cancellationToken);

            if (pending.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "awards-additions",
                    "Awards",
                    pending.Value,
                    "students with pending Award additions",
                    "/StudentAdmin/Awards/Changes",
                    RouteValues: new Dictionary<string, string> { ["Filter"] = AwardsFilter.Additions.ToString() }));
            }
        }

        if (tutorialsTest.Succeeded || adminTest.Succeeded)
        {
            Result<int> requestsForApproval = await _mediator.Send(new CountRequestsPendingApprovalQuery(), cancellationToken);

            if (requestsForApproval.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "tutorials-approval",
                    "Tutorial Requests",
                    requestsForApproval.Value,
                    "Tutorial Requests pending approval",
                    "/Subject/Tutorials/Requests/Index"));
            }

            Result<int> requestsForScheduling = await _mediator.Send(new CountRequestsPendingSchedulingQuery(), cancellationToken);

            if (requestsForScheduling.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "tutorials-scheduling",
                    "Tutorial Requests",
                    requestsForScheduling.Value,
                    "Tutorial Requests pending scheduling",
                    "/Subject/Tutorials/Requests/Index"));
            }
        }

        if (adminTest.Succeeded)
        {
            Result<int> sentralIdRequest = await _mediator.Send(new CountStudentsWithoutSentralIdQuery(), cancellationToken);

            if (sentralIdRequest.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "student-sentralid",
                    "Student Configuration",
                    sentralIdRequest.Value,
                    "students without a linked Sentral Student Id",
                    "/Partner/Students/Reports/WithoutSentralId"));
            }

            Result<(StocktakeEventId EventId, double Percentage)> stocktakeRequest = await _mediator.Send(new CountStocktakeItemsOutstandingQuery(), cancellationToken);

            if (stocktakeRequest.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "stocktake-percentage",
                    "Stocktake",
                    (int)stocktakeRequest.Value.Percentage,
                    "devices remaining to be sighted",
                    "/Equipment/Stocktake/Details",
                    CountDisplay:$"{stocktakeRequest.Value.Percentage.ToString("F", CultureInfo.InvariantCulture)}%",
                    RouteValues: new Dictionary<string, string> { ["id"] = stocktakeRequest.Value.EventId.ToString() }));
            }
        }

        if (enrolmentOfferReviewer.Succeeded)
        {
            Result<int> reviewingResponse = await _mediator.Send(new CountOffersInReviewingResponseStatusQuery(), cancellationToken);

            if (reviewingResponse.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "enrolment-forReview",
                    "Enrolment Responses",
                    reviewingResponse.Value,
                    "pending review",
                    "/Partner/Enrolments/Offers"));
            }
        }

        if (enrolmentOfferApprover.Succeeded)
        {
            Result<int> pendingAcceptance = await _mediator.Send(new CountOffersInPendingAcceptanceStatusQuery(), cancellationToken);

            if (pendingAcceptance.IsSuccess)
            {
                widgets.Add(new CountWidgetModel(
                    "enrolment-forApproval",
                    "Enrolment Responses",
                    pendingAcceptance.Value,
                    "pending approval",
                    "/Partner/Enrolments/Offers"));
            }
        }

        if (enrolmentOfferApprover.Succeeded || enrolmentOfferReviewer.Succeeded)
        {
            Result<List<ChartResponse>> enrolmentsCharts = await _mediator.Send(new GetChartDataForEnrolmentStatusQuery(), cancellationToken);

            if (enrolmentsCharts.IsSuccess)
            {
                foreach (var chart in enrolmentsCharts.Value)
                {
                    widgets.Add(new ChartWidgetModel(
                        $"enrolments-{chart.PeriodId}",
                        "Enrolment Responses",
                        chart.PeriodName,
                        chart.ChartData.Keys.ToList(),
                        chart.ChartData.Values.ToList()));
                }
            }
        }

        return View(widgets);
    }
}