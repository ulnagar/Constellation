namespace Constellation.Application.SchoolContacts.GetContactsBySchool;

using Core.Models.SchoolContacts.Identifiers;
using Core.ValueObjects;
using Schools.Enums;
using System.Collections.Generic;

public sealed record SchoolWithContactsResponse(
    string SchoolCode,
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
        string Role,
        string Note);
}
