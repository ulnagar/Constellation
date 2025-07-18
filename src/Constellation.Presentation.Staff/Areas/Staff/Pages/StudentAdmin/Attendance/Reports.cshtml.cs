namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance;

using Application.Domains.Attendance.Absences.Queries.ExportUnexplainedPartialAbsencesReport;
using Application.Domains.ScheduledReports.Commands.CreateScheduledReport;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForStudent;
using Constellation.Application.Domains.Attendance.Reports.Queries.GenerateHistoricalDailyAttendanceReport;
using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.HistoricalAttendanceReport;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StudentAttendanceReport;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportsModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Reports;
    [ViewData] public string PageTitle => "Attendance Reports";

    public async Task OnGet()
    {
    }

    public async Task<IActionResult> OnGetPartialReport()
    {
        _logger
            .Information("Requested to export Partial Absences report by user {User}", _currentUserService.UserName);

        Result<FileDto> fileRequest = await _mediator.Send(new ExportUnexplainedPartialAbsencesReportCommand());

        if (!fileRequest.IsSuccess)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to export Partial Absences report by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(fileRequest.Error);
            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostAttendanceReport(AttendanceReportSelection viewModel)
    {
        DateOnly startDate = DateOnly.FromDateTime(viewModel.ReportDate);
        startDate = startDate.VerifyStartOfFortnight();
        DateOnly endDate = startDate.AddDays(12);

        GenerateAttendanceReportForStudentQuery command = new(viewModel.StudentId, startDate, endDate);
        
        _logger
            .ForContext(nameof(GenerateAttendanceReportForStudentQuery), command, true)
            .Information("Requested to export Student Absences report by user {User}", _currentUserService.UserName);

        Result<FileDto> fileRequest = await _mediator.Send(command);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed to export Student Absences report by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(fileRequest.Error);

            return Page();
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }

    public async Task<IActionResult> OnPostHistoricAttendanceReport(HistoricalAttendanceReportSelection viewModel)
    {
        GenerateHistoricalDailyAttendanceReportQuery report = new(
            viewModel.Year,
            new(viewModel.StartTerm, viewModel.StartWeek),
            new(viewModel.EndTerm, viewModel.EndWeek));

        Result<EmailRecipient> recipient = EmailRecipient.Create(_currentUserService.UserName, _currentUserService.EmailAddress);

        if (recipient.IsFailure)
        {
            // Log error

            // Modal Error

            return Page();
        }

        CreateScheduledReportCommand<GenerateHistoricalDailyAttendanceReportQuery> command = new(report, recipient.Value);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            // Log error

            // Modal error

            return Page();
        }

        return RedirectToPage();
    }
}