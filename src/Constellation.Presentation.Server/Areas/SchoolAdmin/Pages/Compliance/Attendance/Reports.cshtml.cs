namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Attendance;

using Application.Attendance.GetAttendancePeriodLabels;
using Application.Helpers;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Attendance.GenerateAttendanceReportForPeriod;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;

    public ReportModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage { get; set; } = CompliancePages.Attendance_Reports;

    public List<string> PeriodNames { get; set; } = new();

    [BindProperty]
    public string SelectedPeriod { get; set; }

    public async Task OnGet()
    {
        // Get list of periods
        var request = await _mediator.Send(new GetAttendancePeriodLabelsQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        PeriodNames = request.Value.ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        Result<MemoryStream> request = await _mediator.Send(new GenerateAttendanceReportForPeriodQuery(SelectedPeriod));

        if (request.IsFailure)
        {
            Error = new ErrorDisplay()
            {
                Error = request.Error, 
                RedirectPath = null
            };

            return Page();
        }

        byte[] fileData = request.Value.ToArray();
        string fileName = $"Attendance Report {SelectedPeriod.Replace(",", "")}.xlsx";
        string fileType = FileContentTypes.ExcelModernFile;

        return File(fileData, fileType, fileName);
    }
}