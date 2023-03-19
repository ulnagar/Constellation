namespace Constellation.Application.Casuals.GetCasualsForSelectionList;

using Constellation.Core.Models.Identifiers;

public sealed record CasualsSelectionListResponse(
    CasualId Id,
    string FirstName,
    string LastName);
