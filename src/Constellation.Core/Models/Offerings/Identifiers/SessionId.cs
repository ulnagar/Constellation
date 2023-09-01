namespace Constellation.Core.Models.Offerings.Identifiers;

using System;

public sealed record SessionId(Guid Value)
{
    public static SessionId FromValue(Guid Value) =>
        new(Value);

    public SessionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
