namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;

using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.ValueObjects;
using System;

public sealed record StaffDailyTimetableResponse(
    string PeriodName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    OfferingId OfferingId,
    OfferingName OfferingName,
    string TeamName,
    string TeamLink,
    bool IsCover);
