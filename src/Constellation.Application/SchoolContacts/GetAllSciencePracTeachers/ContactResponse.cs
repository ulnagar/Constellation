namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Core.ValueObjects;
using Core.Models.SchoolContacts.Identifiers;

public sealed record ContactResponse(
    SchoolContactId Id,
    SchoolContactRoleId RoleId,
    Name ContactName,
    EmailAddress ContactEmail,
    PhoneNumber? PhoneNumber,
    bool SelfRegistered,
    string SchoolCode,
    string SchoolName,
    string Note);