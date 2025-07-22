namespace Constellation.Application.Domains.Casuals.Models;

using Core.Models.Identifiers;
using Core.ValueObjects;

public sealed record CasualsListResponse(
    CasualId Id,
    Name Name,
    string SchoolName,
    string EmailAddress,
    string EdvalCode,
    bool IsActive);
