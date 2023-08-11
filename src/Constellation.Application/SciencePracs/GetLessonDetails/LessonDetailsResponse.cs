namespace Constellation.Application.SciencePracs.GetLessonDetails;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record LessonDetailsResponse(
    SciencePracLessonId LessonId,
    int CourseId,
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