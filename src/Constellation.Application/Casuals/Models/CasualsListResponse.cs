namespace Constellation.Application.Casuals.Models;

using Constellation.Core.Models.Identifiers;

public sealed record CasualsListResponse(
    CasualId Id,
    string FirstName,
    string LastName,
    string SchoolName,
    string EmailAddress,
    bool IsActive);
