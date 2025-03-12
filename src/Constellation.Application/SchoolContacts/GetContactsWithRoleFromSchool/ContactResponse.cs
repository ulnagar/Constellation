namespace Constellation.Application.SchoolContacts.GetContactsWithRoleFromSchool;

using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Identifiers;

public sealed record ContactResponse(
    SchoolContactId ContactId,
    SchoolContactRoleId AssignmentId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string EmailAddress,
    Position Position);