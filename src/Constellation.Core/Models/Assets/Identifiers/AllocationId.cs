namespace Constellation.Core.Models.Assets.Identifiers;

using System;

public readonly record struct AllocationId(Guid Value)
{
    public static readonly AllocationId Empty = new(Guid.Empty);

    public static AllocationId FromValue(Guid value) =>
        new(value);

    public AllocationId() 
        : this(Guid.NewGuid()) { }

    public override string ToString() => Value.ToString();
}