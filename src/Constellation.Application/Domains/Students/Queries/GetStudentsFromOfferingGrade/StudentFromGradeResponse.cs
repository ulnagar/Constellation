namespace Constellation.Application.Domains.Students.Queries.GetStudentsFromOfferingGrade;

using Core.Models.Students.Identifiers;
using Core.ValueObjects;

public sealed record StudentFromGradeResponse(
    StudentId StudentId,
    Name Name);