namespace Constellation.Application.SciencePracs.GetLessonsFromCurrentYear;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetLessonsFromCurrentYearQuery
    : IQuery<List<LessonSummaryResponse>>;