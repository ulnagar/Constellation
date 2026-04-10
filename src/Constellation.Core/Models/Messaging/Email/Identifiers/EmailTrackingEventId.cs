namespace Constellation.Core.Models.Messaging.Email.Identifiers;

using Primitives;

public readonly record struct EmailTrackingEventId(Guid Value)
    : IStronglyTypedId<EmailTrackingEventId, Guid>
{
    public static EmailTrackingEventId Empty => new(Guid.Empty);

    public static EmailTrackingEventId FromValue(Guid value) =>
        new(value);

    public EmailTrackingEventId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}