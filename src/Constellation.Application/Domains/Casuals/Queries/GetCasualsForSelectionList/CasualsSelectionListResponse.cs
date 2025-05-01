namespace Constellation.Application.Domains.Casuals.Queries.GetCasualsForSelectionList;

using Core.Models.Identifiers;

public sealed record CasualsSelectionListResponse(
    CasualId Id,
    string FirstName,
    string LastName);
