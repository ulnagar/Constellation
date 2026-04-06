namespace Constellation.Core.Models.Messaging.Drafts.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct QueuedMessageId(Guid Value)
    : IStronglyTypedId
{
    public static QueuedMessageId Empty => new(Guid.Empty);

    public static QueuedMessageId FromValue(Guid value) =>
        new(value);

    public QueuedMessageId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}