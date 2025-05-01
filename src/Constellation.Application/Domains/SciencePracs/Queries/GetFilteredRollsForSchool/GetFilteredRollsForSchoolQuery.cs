namespace Constellation.Application.Domains.SciencePracs.Queries.GetFilteredRollsForSchool;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetFilteredRollsForSchoolQuery(
        string SchoolCode)
    : IQuery<List<RollSummaryResponse>>;
