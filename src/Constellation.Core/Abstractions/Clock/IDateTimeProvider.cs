namespace Constellation.Core.Abstractions.Clock;

using System;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateOnly Today { get; }
    DateOnly Yesterday { get; }
    DateOnly LastDayOfYear { get; }
    DateOnly FirstDayOfYear { get; }

    int CurrentYear { get; }

    DateOnly GetFirstDayOfYear(int year);
    DateOnly GetLastDayOfYear(int year);
}
