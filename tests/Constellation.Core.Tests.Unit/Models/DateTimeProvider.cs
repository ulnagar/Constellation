namespace Constellation.Core.Tests.Unit.Models;

using Abstractions.Clock;

internal class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now { get; } = new DateTime(2024, 09, 11, 17, 54, 00);
    public DateOnly Today { get; } = new DateOnly(2024, 09, 11);
    public DateOnly Yesterday { get; } = new DateOnly(2024, 09, 10);
    public DateOnly LastDayOfYear { get; } = new DateOnly(2024, 12, 31);
    public DateOnly FirstDayOfYear { get; } = new DateOnly(2024, 01, 01);
    public int CurrentYear { get; } = 2024;
    public DateOnly GetFirstDayOfYear(int year) => new(year, 01, 01);

    public DateOnly GetLastDayOfYear(int year) => new(year, 12, 31);
}