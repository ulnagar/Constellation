namespace Constellation.Application.Periods.GetPeriodById;

using System;

public sealed record PeriodResponse(
    int Id,
    int Day,
    int Period,
    string Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    string Type);