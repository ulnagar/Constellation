namespace Constellation.Application.Clock;

using Constellation.Core.Abstractions.Clock;
using System;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    public DateOnly LastDayOfYear => new DateOnly(Today.Year, 12, 31);

    public DateOnly FirstDayOfYear => new DateOnly(Today.Year, 1, 1);
}
