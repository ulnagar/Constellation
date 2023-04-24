namespace Constellation.Core.Models.Identifiers;

using System;

public sealed record StudentAwardId(Guid Value)
{
    public static StudentAwardId FromValue(Guid Value) =>
        new(Value);

    public StudentAwardId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}