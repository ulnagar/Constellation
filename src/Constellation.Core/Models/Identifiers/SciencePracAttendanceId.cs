namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record SciencePracAttendanceId(Guid Value)
{
    public static SciencePracAttendanceId FromValue(Guid Value) =>
        new(Value);

    public SciencePracAttendanceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}