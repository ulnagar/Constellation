namespace Constellation.Application.Periods.UpsertPeriod;

using Abstractions.Messaging;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using System;

public sealed record UpsertPeriodCommand(
    PeriodId Id,
    int Week,
    PeriodDay Day,
    int DaySequence,
    Timetable Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    PeriodType Type)
    : ICommand;
