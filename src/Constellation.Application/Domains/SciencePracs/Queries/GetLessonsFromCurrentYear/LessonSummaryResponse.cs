namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonsFromCurrentYear;

using Core.Models.Identifiers;
using System;

public sealed record LessonSummaryResponse(
    SciencePracLessonId LessonId,
    string CourseName,
    string Name,
    DateOnly DueDate,
    int CompletedRolls,
    int TotalRolls,
    bool Overdue);
