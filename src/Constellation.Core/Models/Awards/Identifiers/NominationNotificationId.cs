namespace Constellation.Core.Models.Awards.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct NominationNotificationId(Guid Value)
    : IStronglyTypedId
{
    public static NominationNotificationId Empty => new(Guid.Empty);

    public static NominationNotificationId FromValue(Guid value) =>
        new(value);

    public NominationNotificationId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}