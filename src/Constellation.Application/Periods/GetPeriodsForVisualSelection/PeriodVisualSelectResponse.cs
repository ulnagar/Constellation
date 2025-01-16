namespace Constellation.Application.Periods.GetPeriodsForVisualSelection;

using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using System;

public sealed record PeriodVisualSelectResponse(
    PeriodId PeriodId,
    Timetable Timetable,
    PeriodWeek Week,
    PeriodDay Day,
    char PeriodCode,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    PeriodType Type,
    int Duration);