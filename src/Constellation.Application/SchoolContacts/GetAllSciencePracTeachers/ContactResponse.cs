namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Core.ValueObjects;

public sealed record ContactResponse(
    int Id,
    int RoleId,
    Name ContactName,
    EmailAddress ContactEmail,
    PhoneNumber? PhoneNumber,
    bool SelfRegistered,
    string SchoolCode,
    string SchoolName);