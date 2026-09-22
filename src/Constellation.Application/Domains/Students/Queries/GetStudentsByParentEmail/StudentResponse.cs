namespace Constellation.Application.Domains.Students.Queries.GetStudentsByParentEmail;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record StudentResponse(
    StudentId StudentId,
    Name Student,
    Grade CurrentGrade,
    bool ResidentialFamily);