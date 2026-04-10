namespace Constellation.Core.Models.Messaging.Tracking.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct TrackingEventId(Guid Value)
    : IStronglyTypedId<TrackingEventId, Guid>
{
    public static TrackingEventId Empty => new(Guid.Empty);

    public static TrackingEventId FromValue(Guid value) =>
        new(value);

    public TrackingEventId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}