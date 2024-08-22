namespace Constellation.Core.Models.Identifiers;

using System;

public readonly record struct SciencePracAttendanceId(Guid Value)
{
    public static SciencePracAttendanceId Empty => new(Guid.Empty);

    public static SciencePracAttendanceId FromValue(Guid value) =>
        new(value);

    public SciencePracAttendanceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}