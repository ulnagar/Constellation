namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Attendance;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Reports.Queries.GenerateAttendanceReportForPeriod;
using Application.Domains.Attendance.Reports.Queries.GenerateCustomReportForPeriod;
using Application.Domains.Attendance.Reports.Queries.GetAttendancePeriodLabels;
using Application.Helpers;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class ReportModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReportModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ReportModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_AttendanceReports;
    [ViewData] public string PageTitle => "Compliance Attendance Reports";

    public List<string> PeriodNames { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Attendance Percentage reports by user {User}", _currentUserService.UserName);
        
        Result<List<string>> request = await _mediator.Send(new GetAttendancePeriodLabelsQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve Attendance Percentage reports by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        PeriodNames = request.Value.ToList();
    }

    public async Task<IActionResult> OnPostAttendanceReport(string selectedPeriod)
    {
        _logger.Information("Requested to export Attendance Percentage Report for period {Period} by user {User}", selectedPeriod, _currentUserService.UserName);

        Result<MemoryStream> request = await _mediator.Send(new GenerateAttendanceReportForPeriodQuery(selectedPeriod));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to export Attendance Percentage Report for period {Period} by user {User}", selectedPeriod, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        byte[] fileData = request.Value.ToArray();
        string fileName = $"Attendance Report {selectedPeriod.Replace(",", "")}.xlsx";
        string fileType = FileContentTypes.ExcelModernFile;

        return File(fileData, fileType, fileName);
    }

    public async Task<IActionResult> OnPostCustomReport(string selectedPeriod, string selectedValue)
    {
        _logger.Information("Requested to export custom Attendance Percentage Report for period {Period} by user {User}", selectedPeriod, _currentUserService.UserName);

        Result<MemoryStream> request = await _mediator.Send(new GenerateCustomReportForPeriodQuery(selectedPeriod, selectedValue));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to export custom Attendance Percentage Report for period {Period} by user {User}", selectedPeriod, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(request.Error);

            return Page();
        }

        byte[] fileData = request.Value.ToArray();
        string fileName = $"Attendance Export {selectedPeriod.Replace(",", "")}.xlsx";
        string fileType = FileContentTypes.ExcelModernFile;

        return File(fileData, fileType, fileName);
    }
}