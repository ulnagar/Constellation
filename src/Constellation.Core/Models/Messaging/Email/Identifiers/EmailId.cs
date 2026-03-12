namespace Constellation.Core.Models.Messaging.Email.Identifiers;

using Constellation.Core.Primitives;
using System;

public readonly record struct EmailId(Guid Value)
    : IStronglyTypedId
{
    public static EmailId Empty => new(Guid.Empty);

    public static EmailId FromValue(Guid value) =>
        new(value);

    public EmailId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}