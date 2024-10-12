namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SciencePracRollId(Guid Value)
    : IStronglyTypedId
{
    public static SciencePracRollId Empty => new(Guid.Empty);

    public static SciencePracRollId FromValue(Guid value) =>
        new(value);

    public SciencePracRollId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
