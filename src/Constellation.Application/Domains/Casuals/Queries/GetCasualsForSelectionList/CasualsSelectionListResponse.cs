namespace Constellation.Application.Domains.Casuals.Queries.GetCasualsForSelectionList;

using Core.Models.Identifiers;
using Core.ValueObjects;

public sealed record CasualsSelectionListResponse(
    CasualId Id,
    Name Name);
