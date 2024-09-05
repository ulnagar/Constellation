namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SciencePracAttendanceId(Guid Value)
    : IStronglyTypedId
{
    public static SciencePracAttendanceId Empty => new(Guid.Empty);

    public static SciencePracAttendanceId FromValue(Guid value) =>
        new(value);

    public SciencePracAttendanceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}