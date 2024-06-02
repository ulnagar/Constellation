namespace Constellation.Application.Students.Models;

using Constellation.Core.Enums;
using Core.ValueObjects;

public sealed record StudentResponse(
    string StudentId,
    Name Name,
    string Gender,
    Grade CurrentGrade,
    string PortalUsername,
    string EmailAddress,
    string School,
    string SchoolCode,
    bool IsDeleted);