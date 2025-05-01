namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactSummary;

using Core.Models.SchoolContacts.Identifiers;

public sealed record ContactSummaryResponse(
    SchoolContactId ContactId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber);
