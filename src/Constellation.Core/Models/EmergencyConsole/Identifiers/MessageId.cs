namespace Constellation.Core.Models.EmergencyConsole.Identifiers;

using Primitives;
using System;

public sealed record MessageId(Guid Value)
    : IStronglyTypedId
{
    public static readonly MessageId Empty = new(Guid.Empty);

    public static MessageId FromValue(Guid value) =>
        new(value);

    public MessageId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}