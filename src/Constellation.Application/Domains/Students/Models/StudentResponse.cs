namespace Constellation.Application.Domains.Students.Models;

using Constellation.Core.Enums;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record StudentResponse(
    StudentId StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    Name Name,
    Gender Gender,
    Grade? Grade,
    EmailAddress EmailAddress,
    string School,
    string SchoolCode,
    bool CurrentEnrolment,
    bool IsDeleted);