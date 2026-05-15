namespace Constellation.Core.Models.Absences.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct AbsenceNotificationId(Guid Value)
    : IStronglyTypedId<AbsenceNotificationId, Guid>
{
    public static AbsenceNotificationId Empty => new(Guid.Empty);

    public static AbsenceNotificationId FromValue(Guid value) =>
        new(value);

    public AbsenceNotificationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}