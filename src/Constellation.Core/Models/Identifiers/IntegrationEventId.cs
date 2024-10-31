namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct IntegrationEventId(Guid Value)
    : IStronglyTypedId
{
    public static IntegrationEventId FromValue(Guid value) =>
        new(value);

    public IntegrationEventId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}