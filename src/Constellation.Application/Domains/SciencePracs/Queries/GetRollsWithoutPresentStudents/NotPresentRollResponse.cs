namespace Constellation.Application.Domains.SciencePracs.Queries.GetRollsWithoutPresentStudents;

using Core.Models.Identifiers;
using System;

public sealed record NotPresentRollResponse(
    string SchoolCode,
    string SchoolName,
    SciencePracLessonId LessonId,
    SciencePracRollId RollId,
    string LessonName,
    string CourseName,
    DateOnly DueDate);