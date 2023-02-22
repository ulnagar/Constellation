namespace Constellation.Application.Casuals.GetCasualById;

using System;

public sealed record CasualResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId);