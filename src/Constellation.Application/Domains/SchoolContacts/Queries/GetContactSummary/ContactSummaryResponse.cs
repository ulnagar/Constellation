namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactSummary;

using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;

public sealed record ContactSummaryResponse(
    SchoolContactId ContactId,
    Name Name,
    EmailAddress EmailAddress,
    PhoneNumber PhoneNumber);
