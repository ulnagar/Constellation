namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonDetails;

using Core.Enums;
using Core.Models.Identifiers;
using Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;

public sealed record LessonDetailsResponse(
    SciencePracLessonId LessonId,
    CourseId CourseId,
    string CourseName,
    string Name,
    DateOnly DueDate,
    List<string> Offerings,
    List<LessonDetailsResponse.LessonRollSummary> Rolls)
{
    public sealed record LessonRollSummary(
        SciencePracRollId RollId,
        string SchoolCode,
        string SchoolName,
        LessonStatus Status,
        int PresentStudents,
        int TotalStudents,
        int NotificationCount,
        bool Overdue);
}