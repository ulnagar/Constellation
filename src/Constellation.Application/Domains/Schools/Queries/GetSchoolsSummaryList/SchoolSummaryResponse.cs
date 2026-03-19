namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsSummaryList;

using Core.Models.Identifiers;

public sealed record SchoolSummaryResponse(
    SchoolCode SchoolCode,
    string Name,
    string Town,
    string PhoneNumber,
    string EmailAddress);