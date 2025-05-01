namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffIntegratedTimetableData;

using Core.Models.Offerings.ValueObjects;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using System;

public sealed record StaffIntegratedTimetableResponse(
    PeriodId PeriodId,
    Timetable Timetable,
    PeriodWeek Week,
    int DayNumber,
    PeriodDay Day,
    string PeriodCode,
    string PeriodName,
    PeriodType Type,
    TimeSpan StartTime,
    TimeSpan EndTime,
    int Duration,
    OfferingName OfferingName);