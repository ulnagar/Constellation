namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Attendance;

using Application.Attendance.GetAttendanceDataForYearFromSentral;
using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Attendance.GetRecentAttendanceValues;
using Application.Common.PresentationModels;
using Constellation.Application.Attendance.GetAttendancePeriodLabels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Attendance;

    public List<AttendanceValueResponse> StudentData { get; set; } = new();

    public List<string> PeriodNames { get; set; } = new();

    [BindProperty] public string SelectedPeriod { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Result<List<AttendanceValueResponse>> request = await _mediator.Send(new GetRecentAttendanceValuesQuery());

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            return;
        }

        StudentData = request.Value;

        Result<List<string>> periodRequest = await _mediator.Send(new GetAttendancePeriodLabelsQuery());

        if (periodRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(periodRequest.Error);

            return;
        }

        PeriodNames = periodRequest.Value.ToList();
    }

    public async Task OnPostSyncAttendancePeriod()
    {
        if (SelectedPeriod == string.Empty)
        {
            ModalContent = new ErrorDisplay(new("", "You must select an Attendance Period to update"));

            return;
        }

        string term = string.Empty;
        string week = string.Empty;
        string year = string.Empty;

        string[] blocks = SelectedPeriod.Split(',');
        foreach (string block in blocks)
        {
            string[] microBlocks = block.Split(' ');

            if (microBlocks.Contains("Term"))
                term = microBlocks[1];
            else if (microBlocks.Contains("Week"))
                week = microBlocks[1];
            else
                year = microBlocks[0];
        }

        await _mediator.Send(new GetAttendanceDataFromSentralQuery(year, term, week));
    }

    public async Task<IActionResult> OnGetSyncAllAttendance()
    {
        await _mediator.Send(new GetAttendanceDataForYearFromSentralCommand());

        return RedirectToPage();
    }
}
