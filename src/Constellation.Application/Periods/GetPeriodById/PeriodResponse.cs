namespace Constellation.Application.Periods.GetPeriodById;

using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using System;

public sealed record PeriodResponse(
    PeriodId Id,
    int Day,
    int Period,
    Timetable Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    PeriodType Type);