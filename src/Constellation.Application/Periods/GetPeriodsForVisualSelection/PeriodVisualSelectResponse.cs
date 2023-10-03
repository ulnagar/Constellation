namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using System;

public sealed record PeriodVisualSelectResponse(
    int PeriodId,
    string Timetable,
    int Day,
    int Period,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    string Type,
    int Duration);