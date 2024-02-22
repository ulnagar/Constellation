namespace Constellation.Core.Models.Rollover;

using Constellation.Core.Enums;
using Enums;

public sealed record RolloverDecision(
    string StudentId,
    string StudentName,
    Grade Grade,
    string SchoolName)
{
    public RolloverStatus Decision { get; set; } = RolloverStatus.Unknown;
}