namespace Constellation.Application.Clock;

using Constellation.Core.Abstractions.Clock;
using System;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now { get => DateTime.Now; }
    public DateOnly Today { get => DateOnly.FromDateTime(DateTime.Today); }
}
