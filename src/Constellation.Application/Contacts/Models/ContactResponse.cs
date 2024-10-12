namespace Constellation.Application.Contacts.Models;

using Constellation.Core.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record ContactResponse(
    StudentId StudentId,
    Name Student,
    Grade StudentGrade,
    string School,
    ContactCategory Category,
    string Contact,
    EmailAddress ContactEmail,
    PhoneNumber ContactPhone,
    string AdditionalNotes);
