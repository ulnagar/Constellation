namespace Constellation.Application.Rollover.ImportStudents;

using Core.Enums;

public sealed record StudentImportRecord(
    string StudentId,
    string FirstName,
    string LastName,
    string PortalUsername,
    Grade Grade,
    string SchoolCode,
    string Gender);