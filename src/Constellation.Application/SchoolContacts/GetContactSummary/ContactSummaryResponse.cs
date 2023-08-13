namespace Constellation.Application.SchoolContacts.GetContactSummary;

public sealed record ContactSummaryResponse(
    int ContactId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string PhoneNumber);
