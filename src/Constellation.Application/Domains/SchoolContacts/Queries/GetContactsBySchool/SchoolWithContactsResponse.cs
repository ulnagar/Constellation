namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactsBySchool;

using Core.Models.Identifiers;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;
using Schools.Enums;
using System.Collections.Generic;

public sealed record SchoolWithContactsResponse(
    SchoolCode SchoolCode,
    string SchoolName,
    SchoolType SchoolType,
    List<SchoolWithContactsResponse.ContactDetails> Contacts)
{
    public sealed record ContactDetails(
        SchoolContactId ContactId,
        SchoolContactRoleId AssignmentId,
        Name Contact,
        EmailAddress EmailAddress,
        PhoneNumber PhoneNumber,
        Position Role,
        string Note);
}
