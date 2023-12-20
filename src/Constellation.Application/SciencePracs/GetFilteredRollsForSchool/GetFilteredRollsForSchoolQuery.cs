namespace Constellation.Application.SciencePracs.GetFilteredRollsForSchool;

using Abstractions.Messaging;
using Constellation.Application.SciencePracs.Models;
using System.Collections.Generic;

public sealed record GetFilteredRollsForSchoolQuery(
        string SchoolCode)
    : IQuery<List<RollSummaryResponse>>;
