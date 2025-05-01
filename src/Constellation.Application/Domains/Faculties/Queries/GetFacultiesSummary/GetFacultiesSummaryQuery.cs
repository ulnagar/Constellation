namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesSummary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFacultiesSummaryQuery()
    : IQuery<List<FacultySummaryResponse>>;