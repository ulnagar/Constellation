namespace Constellation.Core.Models.EmergencyConsole.Identifiers;

using Primitives;
using System;

public readonly record struct MessageId(Guid Value)
    : IStronglyTypedId
{
    public static readonly MessageId Empty = new(Guid.Empty);

    public static MessageId FromValue(Guid value) =>
        new(value);

    public MessageId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}