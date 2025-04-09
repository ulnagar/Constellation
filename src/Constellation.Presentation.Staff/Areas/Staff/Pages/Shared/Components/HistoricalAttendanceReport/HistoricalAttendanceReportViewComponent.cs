namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.HistoricalAttendanceReport;

using Constellation.Core.Abstractions.Clock;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

public sealed class HistoricalAttendanceReportViewComponent
    : ViewComponent
{
    private readonly IDateTimeProvider _dateTime;

    public HistoricalAttendanceReportViewComponent(
        IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public IViewComponentResult Invoke()
    {
        HistoricalAttendanceReportSelection viewModel = new();

        List<string> years = [];

        int currentYear = _dateTime.CurrentYear;
        for (int i = 0; i <= 5; i++)
        {
            years.Add((currentYear - i).ToString(CultureInfo.InvariantCulture));
        }

        viewModel.Years = years;

        return View(viewModel);
    }
}
