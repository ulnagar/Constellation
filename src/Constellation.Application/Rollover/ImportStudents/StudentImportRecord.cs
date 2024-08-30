namespace Constellation.Application.Rollover.ImportStudents;

using Core.Enums;
using Core.Models.Students.ValueObjects;

public sealed record StudentImportRecord(
    StudentReferenceNumber StudentReferenceNumber,
    string FirstName,
    string PreferredName,
    string LastName,
    string EmailAddress,
    Grade Grade,
    string SchoolCode,
    string Gender);