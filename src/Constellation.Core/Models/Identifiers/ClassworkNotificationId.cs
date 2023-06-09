namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record ClassworkNotificationId(Guid Value)
{
    public static ClassworkNotificationId FromValue(Guid Value) =>
        new(Value);

    public ClassworkNotificationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
