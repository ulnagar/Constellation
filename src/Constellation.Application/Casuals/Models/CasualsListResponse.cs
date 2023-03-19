namespace Constellation.Application.Casuals.Models;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record CasualsListResponse(
    CasualId Id,
    string FirstName,
    string LastName,
    string SchoolName,
    string EmailAddress,
    bool IsActive);
