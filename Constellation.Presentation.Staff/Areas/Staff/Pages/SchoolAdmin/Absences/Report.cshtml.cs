namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Pages.Shared.Components.StudentAttendanceReport;
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Report;

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnGetPartialReport()
    {
        Result<FileDto> fileRequest = await _mediator.Send(new ExportUnexplainedPartialAbsencesReportCommand());

        if (fileRequest.IsSuccess)
            return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);

        Error = new()
        {
            Error = fileRequest.Error,
            RedirectPath = null
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAttendanceReport(AttendanceReportSelection viewModel)
    {
        DateOnly startDate = DateOnly.FromDateTime(viewModel.ReportDate);
        startDate = startDate.VerifyStartOfFortnight();
        DateOnly endDate = startDate.AddDays(12);
        
        Result<FileDto> fileRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(viewModel.StudentId, startDate, endDate));

        if (fileRequest.IsFailure)
        {
            Error = new()
            {
                Error = fileRequest.Error,
                RedirectPath = null
            };

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

}