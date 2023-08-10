namespace Constellation.Application.SciencePracs.GetLessonRollsForSchoolsPortal;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetLessonRollsForSchoolQuery(
    string SchoolCode)
    : IQuery<List<ScienceLessonRollSummary>>;