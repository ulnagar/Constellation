namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialDetails;

using Constellation.Core.Enums;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Models.Timetables.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record TutorialDetailsResponse(
    TutorialId Id,
    TutorialName Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    List<TutorialDetailsResponse.StudentSummary> Students,
    List<TutorialDetailsResponse.SessionSummary> Sessions,
    List<TutorialDetailsResponse.ResourceSummary> Resources,
    int Duration)
{
    public sealed record StudentSummary(
        StudentId StudentId,
        StudentReferenceNumber StudentReferenceNumber,
        Gender Gender,
        Name Name,
        Grade? Grade,
        string SchoolCode,
        string SchoolName,
        bool CurrentEnrolment);

    public sealed record SessionSummary(
        TutorialSessionId SessionId,
        string PeriodName,
        string PeriodSortName,
        PeriodWeek Week,
        PeriodDay Day,
        TimeSpan StartTime,
        TimeSpan EndTime,
        int Duration,
        TeacherSummary Teacher);

    public sealed record TeacherSummary(
        StaffId StaffId,
        EmployeeId EmployeeId,
        Name Name);

    public sealed record ResourceSummary(
        TeamsResourceId ResourceId,
        string Name,
        string Url);
}