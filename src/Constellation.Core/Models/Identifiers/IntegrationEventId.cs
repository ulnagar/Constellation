namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct IntegrationEventId(Guid Value)
    : IStronglyTypedId<IntegrationEventId, Guid>
{
    public static IntegrationEventId FromValue(Guid value) =>
        new(value);

    public IntegrationEventId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}