namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonDetails;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;

public sealed record LessonDetailsResponse(
    SciencePracLessonId LessonId,
    string Name,
    DateOnly DueDate,
    List<LessonDetailsResponse.CourseSummary> Courses,
    List<string> Offerings,
    List<LessonDetailsResponse.LessonRollSummary> Rolls)
{
    public sealed record CourseSummary(
        CourseId CourseId,
        string CourseName);

    public sealed record LessonRollSummary(
        SciencePracRollId RollId,
        SchoolCode SchoolCode,
        string SchoolName,
        LessonStatus Status,
        int PresentStudents,
        int TotalStudents,
        int NotificationCount,
        bool Overdue);
}