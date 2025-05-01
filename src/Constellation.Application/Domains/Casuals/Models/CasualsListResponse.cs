namespace Constellation.Application.Domains.Casuals.Models;

using Core.Models.Identifiers;

public sealed record CasualsListResponse(
    CasualId Id,
    string FirstName,
    string LastName,
    string SchoolName,
    string EmailAddress,
    bool IsActive);
