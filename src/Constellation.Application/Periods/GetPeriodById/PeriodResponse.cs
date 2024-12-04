namespace Constellation.Application.Periods.GetPeriodById;

using Core.Models.Timetables.Identifiers;
using System;

public sealed record PeriodResponse(
    PeriodId Id,
    int Day,
    int Period,
    string Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    string Type);