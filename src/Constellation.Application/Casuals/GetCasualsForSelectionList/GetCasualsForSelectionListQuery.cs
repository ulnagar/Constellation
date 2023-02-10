namespace Constellation.Application.Casuals.GetCasualsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCasualsForSelectionListQuery()
    : IQuery<List<CasualsSelectionListResponse>>;