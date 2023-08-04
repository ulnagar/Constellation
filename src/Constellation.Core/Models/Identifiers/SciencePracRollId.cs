namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record SciencePracRollId(Guid Value)
{
    public static SciencePracRollId FromValue(Guid Value) =>
        new(Value);

    public SciencePracRollId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
