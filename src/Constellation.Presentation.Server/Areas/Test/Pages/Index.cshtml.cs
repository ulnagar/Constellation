namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GenerateHistoricalDailyAttendanceReport;
using Application.Interfaces.Gateways;
using BaseModels;
using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Gateways.PowershellGateway;
using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Collections.Generic;
using System.Security;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPowershellGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IPowershellGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _gateway = gateway;
        _logger = logger;
    }

    public List<SentralPeriodAbsenceDto> PageAbsences { get; set; } = new();
    public List<SentralPeriodAbsenceDto> ExportAbsences { get; set; } = new();

    public List<DateOnly> AbsenceDates { get; set; } = new();

    public async Task OnGet()
    {
        //_gateway.Connect("", new SecureString());
        _gateway.GetTeams("");
    }

    public async Task<IActionResult> OnGetHistoricalReport()
    {
        GenerateHistoricalDailyAttendanceReportQuery command = new("2024", new SchoolTermWeek(SchoolTerm.Term1, SchoolWeek.Week1), new SchoolTermWeek(SchoolTerm.Term2, SchoolWeek.Week10));

        Result<FileDto> report = await _mediator.Send(command);

        if (report.IsFailure)
        {
            return Page();
        }

        return File(report.Value.FileData, report.Value.FileType, report.Value.FileName);
    }
}