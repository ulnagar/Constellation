namespace Constellation.Application.Domains.Timetables.Timetables.Queries.GetStaffDailyTimetableData;

using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using System;

public abstract record StaffDailyTimetableResponse(
    string PeriodName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string ActivityName,
    string TeamName,
    string TeamLink,
    bool IsCover);

public sealed record OfferingTimetableResponse(
    string PeriodName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    OfferingId OfferingId,
    OfferingName OfferingName,
    string TeamName,
    string TeamLink,
    bool IsCover) 
    : StaffDailyTimetableResponse(
        PeriodName,
        StartTime,
        EndTime,
        OfferingName.Value,
        TeamName,
        TeamLink,
        IsCover);

public sealed record TutorialTimetableResponse(
    string PeriodName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    TutorialId TutorialId,
    TutorialName TutorialName,
    string TeamName,
    string TeamLink,
    bool IsCover)
    : StaffDailyTimetableResponse(
        PeriodName,
        StartTime,
        EndTime,
        TutorialName.Value,
        TeamName,
        TeamLink,
        IsCover);