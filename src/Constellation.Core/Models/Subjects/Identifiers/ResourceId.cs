namespace Constellation.Core.Models.Subjects.Identifiers;

using System;

public sealed record ResourceId(Guid Value)
{
    public static ResourceId FromValue(Guid Value) =>
        new(Value);

    public ResourceId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
