namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Absences;

using Application.DTOs;
using Application.Extensions;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Absences.ExportUnexplainedPartialAbsencesReport;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Pages.Shared.Components.StudentAttendanceReport;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;

    public ReportModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => AbsencePages.Report;

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