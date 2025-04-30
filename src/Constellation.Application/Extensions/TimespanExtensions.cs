namespace Constellation.Application.Extensions;

using System;

public static class TimespanExtensions
{
    public static string As12HourTime(this TimeSpan time)
    {
        var dt = DateTime.Today.AddTicks(time.Ticks);

        return dt.ToString("hh:mm tt");
    }
}