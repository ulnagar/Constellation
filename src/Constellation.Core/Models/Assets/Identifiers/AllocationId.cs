namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AllocationId(Guid Value)
    : IStronglyTypedId<AllocationId, Guid>
{
    public static readonly AllocationId Empty = new(Guid.Empty);

    public static AllocationId FromValue(Guid value) =>
        new(value);

    public AllocationId() 
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => Value.ToString();
}