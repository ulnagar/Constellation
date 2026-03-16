namespace Constellation.Core.Models.Absences.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AbsenceNotificationId(Guid Value)
    : IStronglyTypedId
{
    public static AbsenceNotificationId FromValue(Guid Value) =>
        new(Value);

    public AbsenceNotificationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}