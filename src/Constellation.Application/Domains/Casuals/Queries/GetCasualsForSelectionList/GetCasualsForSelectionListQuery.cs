namespace Constellation.Application.Domains.Casuals.Queries.GetCasualsForSelectionList;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCasualsForSelectionListQuery()
    : IQuery<List<CasualsSelectionListResponse>>;