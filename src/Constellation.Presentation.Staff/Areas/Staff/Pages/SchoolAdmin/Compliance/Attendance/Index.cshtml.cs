namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Attendance;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;
using Application.Domains.Attendance.Reports.Queries.GetAttendanceDataForYearFromSentral;
using Application.Domains.Attendance.Reports.Queries.GetAttendancePeriodLabels;
using Constellation.Application.Domains.Attendance.Reports.Queries.GetRecentAttendanceValues;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Attendance;
    [ViewData] public string PageTitle => "Compliance Attendance Records";

    public List<AttendanceValueResponse> StudentData { get; set; } = new();

    public List<string> PeriodNames { get; set; } = new();

    [BindProperty] public string SelectedPeriod { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        _logger.Information("Requested to retrieve Compliance Attendance Percentages by user {User}", _currentUserService.UserName);

        Result<List<AttendanceValueResponse>> request = await _mediator.Send(new GetRecentAttendanceValuesQuery());

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve Compliance Attendance Percentages by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(request.Error);

            return;
        }

        StudentData = request.Value;

        Result<List<string>> periodRequest = await _mediator.Send(new GetAttendancePeriodLabelsQuery());

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to retrieve Compliance Attendance Percentages by user {User}", _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(periodRequest.Error);

            return;
        }

        PeriodNames = periodRequest.Value.ToList();
    }

    public async Task<IActionResult> OnPostSyncAttendancePeriod()
    {
        if (string.IsNullOrWhiteSpace(SelectedPeriod))
        {
            ModalContent = ErrorDisplay.Create(new("", "You must select an Attendance Period to update"));

            return Page();
        }

        _logger.Information("Requested to update Attendance Percentage for period {Period} by user {User}", SelectedPeriod, _currentUserService.UserName);

        SchoolTerm term = SchoolTerm.Empty;
        SchoolWeek week = SchoolWeek.Empty;
        string year = string.Empty;

        string[] blocks = SelectedPeriod.Split(',');
        foreach (string block in blocks)
        {
            string preparedBlock = block.Trim();

            if (preparedBlock.Contains("Term", StringComparison.InvariantCultureIgnoreCase))
                term = SchoolTerm.FromName(preparedBlock) ?? SchoolTerm.Empty;
            else if (preparedBlock.Contains("Week", StringComparison.InvariantCultureIgnoreCase))
                week = SchoolWeek.FromName(preparedBlock) ?? SchoolWeek.Empty;
            else
                year = preparedBlock;
        }

        Result result = await _mediator.Send(new UpdateAttendanceDataForPeriodFromSentralCommand(year, term, week));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Attendance Percentage for period {Period} by user {User}", SelectedPeriod, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetSyncAllAttendance()
    {
        _logger.Information("Requested to update all Attendance Percentage periods by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(new GetAttendanceDataForYearFromSentralCommand());

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update all Attendance Percentage periods by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            return Page();
        }

        return RedirectToPage();
    }
}
