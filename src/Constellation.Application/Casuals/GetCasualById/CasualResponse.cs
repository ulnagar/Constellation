namespace Constellation.Application.Casuals.GetCasualById;

using Constellation.Core.Models.Identifiers;

public sealed record CasualResponse(
    CasualId Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string AdobeConnectId);