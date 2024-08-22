namespace Constellation.Core.Models.Identifiers;

using System;

public readonly record struct SciencePracRollId(Guid Value)
{
    public static SciencePracRollId Empty => new(Guid.Empty);

    public static SciencePracRollId FromValue(Guid value) =>
        new(value);

    public SciencePracRollId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
