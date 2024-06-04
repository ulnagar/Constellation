namespace Constellation.Application.Schools.GetSchoolsSummaryList;

public sealed record SchoolSummaryResponse(
    string SchoolCode,
    string Name,
    string Town,
    string PhoneNumber,
    string EmailAddress);