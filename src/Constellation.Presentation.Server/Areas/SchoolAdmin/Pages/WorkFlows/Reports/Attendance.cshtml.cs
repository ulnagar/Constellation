namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.WorkFlows.Reports;

using Application.Attendance.GetAttendanceTrendValues;
using Application.Models.Auth;
using Application.WorkFlows.CreateAttendanceCase;
using Application.WorkFlows.OpenAttendanceCaseExistsForStudent;
using Application.WorkFlows.UpdateAttendanceCaseDetails;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflows;

[Authorize(Policy = AuthPolicies.CanManageWorkflows)]
public class AttendanceModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public AttendanceModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => WorkFlowPages.Reports;

    public List<AttendanceTrend> Students { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<AttendanceTrend>> studentAttendanceTrends = await _mediator.Send(new GetAttendanceTrendValuesQuery());

        if (studentAttendanceTrends.IsFailure)
        {
            Error = new()
            {
                Error = studentAttendanceTrends.Error,
                RedirectPath = null
            };

            return;
        }

        Students = studentAttendanceTrends.Value
            .OrderBy(entry => entry.Severity)
            .ThenBy(entry => entry.Grade)
            .ThenBy(entry => entry.Name.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPostCreateWorkFlows(Dictionary<string, ActionType> entries)
    {
        return RedirectToPage("/WorkFlows/Reports/Index", new { area = "SchoolAdmin" });

        foreach (KeyValuePair<string, ActionType> entry in entries)
        {
            Result result = entry.Value switch
            {
                ActionType.Create => await _mediator.Send(new CreateAttendanceCaseCommand(entry.Key)),
                ActionType.Update => await _mediator.Send(new UpdateAttendanceCaseDetailsCommand(entry.Key)),
                _ => Result.Failure(Core.Shared.Error.NullValue)
            };

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/WorkFlows/Reports/Index", values: new { area = "SchoolAdmin" })
                };

                return Page();
            }
        }
        
        return RedirectToPage("/WorkFlows/Reports/Index", new { area = "SchoolAdmin" });
    }

    public enum ActionType
    {
        Create,
        Update
    }
}