namespace Constellation.Application.Domains.Schools.Queries.GetSchoolContactDetails;

using Core.Models.Identifiers;

public sealed record SchoolContactDetailsResponse(
    SchoolCode Code,
    string Name,
    string Address,
    string Town,
    string State,
    string PostCode,
    string PhoneNumber,
    string FaxNumber,
    string EmailAddress);