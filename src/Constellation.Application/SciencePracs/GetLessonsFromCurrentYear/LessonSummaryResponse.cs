namespace Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record LessonSummaryResponse(
    SciencePracLessonId LessonId,
    string Name,
    DateOnly DueDate,
    int OutstandingRolls,
    int TotalRolls);
