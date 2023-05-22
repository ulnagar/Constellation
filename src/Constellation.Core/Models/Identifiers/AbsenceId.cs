namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record AbsenceId(Guid Value)
{
    public static AbsenceId FromValue(Guid Value) =>
        new(Value);

    public AbsenceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
