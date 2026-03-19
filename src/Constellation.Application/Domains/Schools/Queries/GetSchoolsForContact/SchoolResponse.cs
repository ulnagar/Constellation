namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsForContact;

using Core.Models.Identifiers;

public sealed record SchoolResponse(
    SchoolCode SchoolCode,
    string Name);
