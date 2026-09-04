namespace Constellation.Application.Domains.Students.Models;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record StudentLoginData(
    StudentReferenceNumber StudentReferenceNumber,
    Name Student,
    Grade Grade,
    EmailAddress EmailAddress,
    string SchoolName,
    DateTime? LastLoginTime);