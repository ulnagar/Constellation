namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Plans;

using Application.Attendance.Plans.GetAttendancePlansSummary;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Plans;
    [ViewData] public string PageTitle => "Attendance Plans";

    public List<AttendancePlanSummaryResponse> Plans { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<AttendancePlanSummaryResponse>> plans = await _mediator.Send(new GetAttendancePlansSummaryQuery());

        if (plans.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), plans.Error, true)
                .Warning("Failed to retrieve Attendance Plans by user {User}", _currentUserService.UserName);

            return;
        }

        Plans = plans.Value;
    }
}
