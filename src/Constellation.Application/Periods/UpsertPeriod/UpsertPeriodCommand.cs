namespace Constellation.Application.Periods.UpsertPeriod;

using Abstractions.Messaging;
using System;

public sealed record UpsertPeriodCommand(
    int? Id,
    int Day,
    int Period,
    string Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    string Type)
    : ICommand;
