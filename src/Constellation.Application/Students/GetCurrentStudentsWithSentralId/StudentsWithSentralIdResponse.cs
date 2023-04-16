namespace Constellation.Application.Students.GetCurrentStudentsWithSentralId;

using Constellation.Core.Enums;
using Constellation.Core.ValueObjects;

public sealed record StudentWithSentralIdResponse(
    string StudentId,
    Name Name,
    Grade Grade,
    string SentralId);
