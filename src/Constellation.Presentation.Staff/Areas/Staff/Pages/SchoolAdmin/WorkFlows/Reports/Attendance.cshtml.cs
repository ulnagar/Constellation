namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Reports;

using Application.Attendance.GetAttendanceTrendValues;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.WorkFlows.CreateAttendanceCase;
using Application.WorkFlows.UpdateAttendanceCaseDetails;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class AttendanceModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;

    public AttendanceModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Reports;

    public List<AttendanceTrend> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<AttendanceTrend>> studentAttendanceTrends = await _mediator.Send(new GetAttendanceTrendValuesQuery());

        if (studentAttendanceTrends.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentAttendanceTrends.Error);

            return;
        }

        Students = studentAttendanceTrends.Value
            .OrderBy(entry => entry.Severity)
            .ThenBy(entry => entry.Grade)
            .ThenBy(entry => entry.Name.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPostCreateWorkFlows(List<WorkFlowNeeded> entries)
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Auth.NotAuthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Reports/Index", values: new { area = "Staff" }));

            return Page();
        }

        foreach (WorkFlowNeeded entry in entries)
        {
            Result result = entry.Type switch
            {
                WorkFlowNeeded.ActionType.Create => await _mediator.Send(new CreateAttendanceCaseCommand(entry.StudentId)),
                WorkFlowNeeded.ActionType.Update => await _mediator.Send(new UpdateAttendanceCaseDetailsCommand(entry.StudentId)),
                _ => Result.Failure(Core.Shared.Error.NullValue)
            };

            if (result.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Reports/Index", values: new { area = "Staff" }));

                return Page();
            }
        }
        
        return RedirectToPage("/SchoolAdmin/WorkFlows/Reports/Index", new { area = "Staff" });
    }

    public class WorkFlowNeeded
    {
        public string StudentId { get; set; }
        public ActionType Type { get; set; }

        public enum ActionType
        {
            Create,
            Update
        }
    }
    
}