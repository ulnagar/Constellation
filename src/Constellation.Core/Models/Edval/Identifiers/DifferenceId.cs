namespace Constellation.Core.Models.Edval.Identifiers;

using Primitives;
using System;

public sealed record DifferenceId(Guid Value) : IStronglyTypedId
{
    public static DifferenceId FromValue(Guid value) =>
        new(value);

    public DifferenceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
