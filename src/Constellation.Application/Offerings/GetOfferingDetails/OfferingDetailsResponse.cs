namespace Constellation.Application.Offerings.GetOfferingDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record OfferingDetailsResponse(
    OfferingId Id,
    OfferingName Name,
    int CourseId,
    string CourseName,
    Grade CourseGrade,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    List<OfferingDetailsResponse.StudentSummary> Students,
    List<OfferingDetailsResponse.SessionSummary> Sessions,
    List<OfferingDetailsResponse.LessonSummary> Lessons,
    List<OfferingDetailsResponse.TeacherSummary> Teachers,
    List<OfferingDetailsResponse.ResourceSummary> Resources,
    int FTETotal,
    int Duration)
{
    public sealed record StudentSummary(
        string StudentId,
        string Gender,
        Name Name,
        Grade Grade,
        string SchoolCode,
        string SchoolName);

    public sealed record SessionSummary(
        SessionId SessionId,
        int PeriodId,
        string PeriodName,
        string PeriodSortName,
        int Duration);

    public sealed record TeacherSummary(
        string StaffId,
        Name Name,
        AssignmentType Type);

    public sealed record ResourceSummary(
        ResourceId ResourceId,
        ResourceType Type,
        string Name,
        string Url);

    public sealed record LessonSummary(
        SciencePracLessonId LessonId,
        DateOnly DueDate,
        string Name,
        List<LessonStudentAttendance> Students);

    public sealed record LessonStudentAttendance(
        Name Name,
        string SchoolName,
        LessonStatus Status,
        bool WasPresent,
        string Comment);
}
