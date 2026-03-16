namespace Constellation.Core.Models.Edval.Identifiers;

using Primitives;
using System;

public readonly record struct DifferenceId(Guid Value) : IStronglyTypedId
{
    public static DifferenceId FromValue(Guid value) =>
        new(value);

    public DifferenceId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
