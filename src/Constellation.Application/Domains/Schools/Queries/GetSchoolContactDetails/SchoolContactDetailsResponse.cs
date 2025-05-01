namespace Constellation.Application.Domains.Schools.Queries.GetSchoolContactDetails;

public sealed record SchoolContactDetailsResponse(
    string Code,
    string Name,
    string Address,
    string Town,
    string State,
    string PostCode,
    string PhoneNumber,
    string FaxNumber,
    string EmailAddress);