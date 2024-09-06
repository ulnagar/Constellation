namespace Constellation.Application.Students.GetFilteredStudents;

using Core.Enums;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record FilteredStudentResponse(
    StudentId StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    Name StudentName,
    Gender Gender,
    Grade? Grade,
    string School,
    string SchoolCode,
    int EnrolmentCount,
    bool IsDeleted);
