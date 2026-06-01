namespace Constellation.Application.Domains.Students.Queries.GetFilteredStudents;

using Core.Enums;
using Core.Models.Common.Enums;
using Core.Models.Identifiers;
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
    SchoolCode SchoolCode,
    int EnrolmentCount,
    bool CurrentEnrolment,
    bool IsDeleted);
