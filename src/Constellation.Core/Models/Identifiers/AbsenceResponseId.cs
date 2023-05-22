namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record AbsenceResponseId(Guid Value)
{
    public static AbsenceResponseId FromValue(Guid Value) =>
        new(Value);

    public AbsenceResponseId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
