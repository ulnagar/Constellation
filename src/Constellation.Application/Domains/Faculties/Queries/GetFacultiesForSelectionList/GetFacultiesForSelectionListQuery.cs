namespace Constellation.Application.Domains.Faculties.Queries.GetFacultiesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFacultiesForSelectionListQuery()
    : IQuery<List<FacultySummaryResponse>>;