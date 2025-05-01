namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollsForSchoolsPortal;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetLessonRollsForSchoolQuery(
    string SchoolCode)
    : IQuery<List<ScienceLessonRollSummary>>;