namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.Models.Timetables.Identifiers;
using System;
using System.Collections.Generic;

public sealed record OfferingDetailsResponse(
    OfferingId Id,
    OfferingName Name,
    CourseId CourseId,
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
    decimal FTETotal,
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
        SessionId SessionId,
        PeriodId PeriodId,
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
