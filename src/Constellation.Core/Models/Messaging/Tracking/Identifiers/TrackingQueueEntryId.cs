namespace Constellation.Core.Models.Messaging.Tracking.Identifiers;

using Primitives;

public readonly record struct TrackingQueueEntryId(Guid Value)
    : IStronglyTypedId
{
    public static TrackingQueueEntryId Empty => new(Guid.Empty);

    public static TrackingQueueEntryId FromValue(Guid value) =>
        new(value);

    public TrackingQueueEntryId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => 
        Value.ToString();
}