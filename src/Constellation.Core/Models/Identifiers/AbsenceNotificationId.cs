namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record AbsenceNotificationId(Guid Value)
{
    public static AbsenceNotificationId FromValue(Guid Value) =>
        new(Value);

    public AbsenceNotificationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}