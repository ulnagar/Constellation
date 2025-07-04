namespace Constellation.Application.Domains.Casuals.Queries.GetCasualById;

using Core.Models.Identifiers;

public sealed record CasualResponse(
    CasualId Id,
    string FirstName,
    string LastName,
    string EmailAddress,
    string SchoolCode,
    string EdvalTeacherCode);