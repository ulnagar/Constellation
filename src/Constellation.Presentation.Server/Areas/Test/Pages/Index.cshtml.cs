namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GenerateHistoricalDailyAttendanceReport;
using BaseModels;
using Constellation.Application.Helpers;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet()
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