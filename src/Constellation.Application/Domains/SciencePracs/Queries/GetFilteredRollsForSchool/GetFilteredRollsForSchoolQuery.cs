namespace Constellation.Application.Domains.SciencePracs.Queries.GetFilteredRollsForSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetFilteredRollsForSchoolQuery(
        SchoolCode SchoolCode)
    : IQuery<List<RollSummaryResponse>>;
