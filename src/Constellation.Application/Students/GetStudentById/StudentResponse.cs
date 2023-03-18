namespace Constellation.Application.Students.GetStudentById;

using Constellation.Core.Enums;

public sealed record StudentResponse(
    string StudentId,
    string DisplayName,
    Grade CurrentGrade,
    string EmailAddress,
    string School,
    string SchoolCode,
    bool IsDeleted)