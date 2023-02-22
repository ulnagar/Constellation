namespace Constellation.Application.Casuals.Models;

using System;

public sealed record CasualsListResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string SchoolName,
    string EmailAddress,
    bool IsActive);
