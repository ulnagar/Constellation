namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Absences;

using Application.Common.PresentationModels;
using Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Components.StudentAttendanceReport;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;

    public ReportModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Absences_Report;

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnGetPartialReport()
    {
        Result<FileDto> fileRequest = await _mediator.Send(new ExportUnexplainedPartialAbsencesReportCommand());

        if (fileRequest.IsSuccess)
            return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);

        ModalContent = new ErrorDisplay(fileRequest.Error);

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
            ModalContent = new ErrorDisplay(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

}