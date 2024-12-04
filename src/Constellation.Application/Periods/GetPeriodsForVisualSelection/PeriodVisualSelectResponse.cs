namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using Core.Models.Timetables.Identifiers;
using System;

public sealed record PeriodVisualSelectResponse(
    PeriodId PeriodId,
    string Timetable,
    int Day,
    int Period,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    string Type,
    int Duration);