namespace Constellation.Core.Models.Messaging.Drafts.Identifiers;

using Primitives;

public readonly record struct MessageRecipientId(Guid Value)
    : IStronglyTypedId
{
    public static MessageRecipientId Empty => new(Guid.Empty);

    public static MessageRecipientId FromValue(Guid value) =>
        new(value);

    public MessageRecipientId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}