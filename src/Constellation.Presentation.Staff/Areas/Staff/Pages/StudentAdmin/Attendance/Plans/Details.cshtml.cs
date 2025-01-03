namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Plans;

using Application.Attendance.Plans.AddAttendancePlanNote;
using Application.Attendance.Plans.GetAttendancePlanDetails;
using Application.Attendance.Plans.UpdateAttendancePlanStatus;
using Application.Common.PresentationModels;
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
using Shared.Components.AddAttendancePlanNote;
using Shared.Components.UpdateAttendancePlanStatus;

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

    public async Task OnGet() => await PreparePage();

    private async Task<IActionResult> PreparePage()
    {
        if (Id == AttendancePlanId.Empty)
        {
            return RedirectToPage("/StudentAdmin/Attendance/Plans/Index", new { area = "Staff" });
        }

        GetAttendancePlanDetailsQuery request = new(Id);

        _logger
            .ForContext(nameof(GetAttendancePlanDetailsQuery), request, true)
            .Information("Requested to retrieve Attendance Plan with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<AttendancePlanDetailsResponse> plan = await _mediator.Send(request);

        if (plan.IsFailure)
        {
            _logger
                .ForContext(nameof(GetAttendancePlanDetailsQuery), request, true)
                .ForContext(nameof(Error), plan.Error, true)
                .Warning("Failed to retrieve Attendance Plan with id {Id} by user {User}", Id, _currentUserService.UserName);

            return RedirectToPage("/StudentAdmin/Attendance/Plans/Index", new { area = "Staff" });
        }

        Plan = plan.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatus(UpdateAttendancePlanStatusSelection viewModel)
    {
        UpdateAttendancePlanStatusCommand command = new(Id, viewModel.SelectedStatus);

        _logger
            .ForContext(nameof(UpdateAttendancePlanStatusCommand), command, true)
            .Information("Requested to update status of Attendance Plan by user {User}", _currentUserService.UserName);

        Result update = await _mediator.Send(command);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateAttendancePlanStatusCommand), command, true)
                .ForContext(nameof(Error), update.Error, true)
                .Information("Failed to update status of Attendance Plan by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                update.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            return await PreparePage();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddNote(AddAttendancePlanNoteSelection viewModel)
    {
        AddAttendancePlanNoteCommand command = new(Id, viewModel.Note);

        _logger
            .ForContext(nameof(AddAttendancePlanNoteCommand), command, true)
            .Information("Requested to add note to Attendance Plan by user {User}", _currentUserService.UserName);

        Result update = await _mediator.Send(command);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(AddAttendancePlanNoteCommand), command, true)
                .ForContext(nameof(Error), update.Error, true)
                .Information("Failed to add note to Attendance Plan by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                update.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            return await PreparePage();
        }

        return RedirectToPage();
    }
}