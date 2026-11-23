namespace Constellation.Core.Models.Rollover;

using Constellation.Core.Enums;
using Constellation.Core.Models.Students.Identifiers;
using Enums;
using Students.ValueObjects;

public sealed record RolloverDecision(
    StudentId StudentId,
    StudentReferenceNumber StudentReferenceNumber,
    string StudentName,
    Grade Grade,
    string SchoolName)
{
    public RolloverStatus Decision { get; set; } = RolloverStatus.Unknown;
}