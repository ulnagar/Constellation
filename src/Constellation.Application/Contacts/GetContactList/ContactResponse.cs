namespace Constellation.Application.Contacts.GetContactList;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;

public sealed record ContactResponse(
    string StudentId,
    Name Student,
    Grade StudentGrade,
    string School,
    ContactCategory Category,
    string Contact,
    EmailAddress ContactEmail,
    PhoneNumber? ContactPhone);
