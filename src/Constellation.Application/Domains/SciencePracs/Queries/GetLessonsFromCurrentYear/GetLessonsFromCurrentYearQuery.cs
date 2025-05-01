namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonsFromCurrentYear;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetLessonsFromCurrentYearQuery
    : IQuery<List<LessonSummaryResponse>>;