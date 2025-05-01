namespace Constellation.Application.Domains.SciencePracs.Queries.GenerateOverdueReport;

using System;

public sealed record OverdueRollResponse(
    string SchoolName,
    string LessonName,
    string SubjectName,
    DateTime DueDate);