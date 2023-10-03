namespace Constellation.Core.Abstractions.Clock;

using System;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateOnly Today { get; }
    DateOnly LastDayOfYear { get; }
    DateOnly FirstDayOfYear { get; }
}
