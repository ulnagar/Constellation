namespace Constellation.Application.Casuals.GetCasualsForSelectionList;

using System;

public sealed record CasualsSelectionListResponse(
    Guid Id,
    string FirstName,
    string LastName);
