namespace Constellation.Application.Students.GetCurrentStudentsWithSentralId;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Identifiers;

public sealed record StudentWithSentralIdResponse(
    StudentId StudentId,
    Name Name,
    Grade Grade,
    string SentralId);
