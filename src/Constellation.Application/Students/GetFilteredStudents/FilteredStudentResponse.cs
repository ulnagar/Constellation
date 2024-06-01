namespace Constellation.Application.Students.GetFilteredStudents;

using Core.Enums;
using Core.ValueObjects;

public sealed record FilteredStudentResponse(
    string StudentId,
    Name StudentName,
    string Gender,
    Grade Grade,
    string School,
    string SchoolCode,
    int EnrolmentCount,
    bool IsDeleted);
