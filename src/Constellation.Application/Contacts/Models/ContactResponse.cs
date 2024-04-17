namespace Constellation.Application.Contacts.Models;

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
    PhoneNumber ContactPhone);
