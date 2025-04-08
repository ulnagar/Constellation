namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GenerateHistoricalDailyAttendanceReport;
using BaseModels;
using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Students.ValueObjects;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static Constellation.Core.Errors.DomainErrors.LinkedSystems;
using System.Collections.Generic;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Enums;
using System.Diagnostics;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _gateway;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        IStudentRepository studentRepository,
        ISentralGateway gateway,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _studentRepository = studentRepository;
        _gateway = gateway;
        _logger = logger;
    }

    public List<SentralPeriodAbsenceDto> PageAbsences { get; set; } = new();
    public List<SentralPeriodAbsenceDto> ExportAbsences { get; set; } = new();

    public List<DateOnly> AbsenceDates { get; set; } = new();

    public async Task OnGet()
    {
        var srn = StudentReferenceNumber.FromValue("450978264");
        var student = await _studentRepository.GetBySRN(srn);
        var sentralId = student.SystemLinks.FirstOrDefault(entry => entry.System == SystemType.Sentral)?.Value;

        var pageTimer = new Stopwatch();
        pageTimer.Start();
        var pageReadAbsences = await _gateway.GetPartialAbsenceDataAsync(sentralId);
        pageTimer.Stop();

        var exportTimer = new Stopwatch();
        exportTimer.Start();
        var exportAbsences = await _gateway.GetAttendanceModuleAbsenceDataForSchool();
        exportTimer.Stop();

        PageAbsences = pageReadAbsences;
        ExportAbsences = exportAbsences[srn];

        var pageDates = PageAbsences.Select(entry => entry.Date);
        var exportDates = ExportAbsences.Select(entry => entry.Date);

        AbsenceDates = pageDates.Concat(exportDates).Distinct().OrderBy(entry => entry).ToList();
    }

    public async Task<IActionResult> OnGetHistoricalReport()
    {
        GenerateHistoricalDailyAttendanceReportCommand command = new(new DateOnly(2024, 1, 1), new DateOnly(2024, 7, 7));

        Result<MemoryStream> report = await _mediator.Send(command);

        if (report.IsFailure)
        {
            return Page();
        }

        return File(report.Value, FileContentTypes.ExcelModernFile, "Sef Attendance Data.xlsx");
    }
}