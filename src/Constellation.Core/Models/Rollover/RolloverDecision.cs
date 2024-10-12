namespace Constellation.Core.Models.Rollover;

using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Enums;

public sealed record RolloverDecision(
    StudentId StudentId,
    string StudentName,
    Grade Grade,
    string SchoolName)
{
    public RolloverStatus Decision { get; set; } = RolloverStatus.Unknown;
}