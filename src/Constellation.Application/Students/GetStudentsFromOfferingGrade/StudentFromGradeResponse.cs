namespace Constellation.Application.Students.GetStudentsFromOfferingGrade;

using Core.ValueObjects;

public sealed record StudentFromGradeResponse(
    string StudentId,
    Name Name);