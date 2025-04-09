namespace Constellation.Application.Clock;

using Constellation.Core.Abstractions.Clock;
using System;
using System.Globalization;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    public DateOnly Yesterday => DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

    public DateOnly LastDayOfYear => new DateOnly(CurrentYear, 12, 31);

    public DateOnly FirstDayOfYear => new DateOnly(CurrentYear, 1, 1);

    public int CurrentYear => Today.Year;
    public string CurrentYearAsString => Today.ToString("yyyy", CultureInfo.InvariantCulture);

    public DateOnly GetFirstDayOfYear(int year) => new DateOnly(year, 1, 1);

    public DateOnly GetLastDayOfYear(int year) => new DateOnly(year, 12, 31);
}
