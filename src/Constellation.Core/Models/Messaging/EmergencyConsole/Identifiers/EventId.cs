namespace Constellation.Core.Models.Messaging.EmergencyConsole.Identifiers;

using Primitives;

public readonly record struct EventId(Guid Value)
    : IStronglyTypedId
{
    public static readonly EventId Empty = new(Guid.Empty);

    public static EventId FromValue(Guid value) =>
        new(value);

    public EventId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() => 
        Value.ToString();
}