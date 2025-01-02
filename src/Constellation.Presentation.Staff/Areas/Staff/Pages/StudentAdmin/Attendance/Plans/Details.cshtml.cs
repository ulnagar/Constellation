namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Plans;

using Application.Attendance.Plans.GetAttendancePlanDetails;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Attendance.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Plans;
    [ViewData] public string PageTitle => "Attendance Plan Details";

    [BindProperty(SupportsGet = true)]
    public AttendancePlanId Id { get; set; } = AttendancePlanId.Empty;

    public AttendancePlanDetailsResponse Plan { get; set; }

    public async Task OnGet()
    {
        if (Id == AttendancePlanId.Empty)
        {
            return;
        }

        Result<AttendancePlanDetailsResponse> plan = await _mediator.Send(new GetAttendancePlanDetailsQuery(Id));

        if (plan.IsFailure)
        {
            return;
        }

        Plan = plan.Value;
    }
}