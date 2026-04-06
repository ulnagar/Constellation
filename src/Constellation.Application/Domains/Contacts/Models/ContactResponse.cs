namespace Constellation.Application.Domains.Contacts.Models;

using Core.Enums;
using Core.Models.Students.ValueObjects;
using Core.Primitives;
using Core.ValueObjects;

public sealed record ContactResponse(
    StudentReferenceNumber StudentId,
    Name Student,
    Grade StudentGrade,
    string School,
    ContactCategory Category,
    IStronglyTypedId Id,
    string Contact,
    EmailAddress ContactEmail,
    PhoneNumber? ContactPhone,
    string AdditionalNotes);
