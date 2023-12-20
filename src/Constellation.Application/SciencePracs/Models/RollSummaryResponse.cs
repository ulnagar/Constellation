namespace Constellation.Application.SciencePracs.Models;

using Core.Enums;
using Core.Models.Identifiers;
using System;

public sealed record RollSummaryResponse(
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    string LessonName,
    string CourseName,
    DateOnly DueDate,
    LessonStatus Status,
    int PresentStudents,
    int TotalStudents,
    bool Overdue);