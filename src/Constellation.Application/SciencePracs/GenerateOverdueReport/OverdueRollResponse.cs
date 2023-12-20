namespace Constellation.Application.SciencePracs.GenerateOverdueReport;

using System;

public sealed record OverdueRollResponse(
    string SchoolName,
    string LessonName,
    string SubjectName,
    DateTime DueDate);