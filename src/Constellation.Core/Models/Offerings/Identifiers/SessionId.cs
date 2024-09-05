namespace Constellation.Core.Models.Offerings.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct SessionId(Guid Value)
    : IStronglyTypedId
{
    public static SessionId Empty => new(Guid.Empty);

    public static SessionId FromValue(Guid value) =>
        new(value);

    public SessionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
