namespace Constellation.Application.Domains.SciencePracs.Queries.GetLessonRollsForSchoolsPortal;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetLessonRollsForSchoolQuery(
    SchoolCode SchoolCode)
    : IQuery<List<ScienceLessonRollSummary>>;