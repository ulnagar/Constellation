namespace Constellation.Application.Students.GetStudentsFromOfferingGrade;

using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record StudentFromGradeResponse(
    StudentId StudentId,
    Name Name);