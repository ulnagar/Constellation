namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record AbsenceNotificationId(Guid Value)
    : IStronglyTypedId
{
    public static AbsenceNotificationId FromValue(Guid Value) =>
        new(Value);

    public AbsenceNotificationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}