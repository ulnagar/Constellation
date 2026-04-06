namespace Constellation.Application.Domains.Schools.Queries.GetSchoolById;

using Core.Models.Identifiers;

public sealed record SchoolResponse(
    SchoolCode SchoolCode,
    string Name);