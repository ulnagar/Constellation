namespace Constellation.Core.Models.Assets.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct LocationId(Guid Value)
    : IStronglyTypedId<LocationId, Guid>
{
    public static LocationId Empty => new(Guid.Empty);

    public static LocationId FromValue(Guid value) =>
        new(value);

    public LocationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => Value.ToString();
}