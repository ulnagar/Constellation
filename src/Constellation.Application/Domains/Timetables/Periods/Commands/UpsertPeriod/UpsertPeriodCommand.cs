namespace Constellation.Application.Domains.Timetables.Periods.Commands.UpsertPeriod;

using Abstractions.Messaging;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using System;

public sealed record UpsertPeriodCommand(
    PeriodId Id,
    PeriodWeek Week,
    PeriodDay Day,
    char PeriodCode,
    Timetable Timetable,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Name,
    PeriodType Type)
    : ICommand;
