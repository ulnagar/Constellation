namespace Constellation.Application.SchoolContacts.GetAllContacts;

using Core.Models.SchoolContacts.Identifiers;

public sealed record SchoolContactResponse(
    SchoolContactId Id,
    SchoolContactRoleId AssignmentId,
    string Name,
    string EmailAddress,
    string PhoneNumber,
    bool IsDirectNumber,
    string Role,
    string SchoolName,
    bool IsActivePartnerSchool,
    string Note);
