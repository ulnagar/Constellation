namespace Constellation.Application.Faculties.GetFacultiesSummary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFacultiesSummaryQuery()
    : IQuery<List<FacultySummaryResponse>>;