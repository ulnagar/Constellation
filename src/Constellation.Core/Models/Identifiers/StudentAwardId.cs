namespace Constellation.Core.Models.Identifiers;

using System;

public record struct StudentAwardId(Guid Value)
{
    public static StudentAwardId Empty => new(Guid.Empty);

    public static StudentAwardId FromValue(Guid value) =>
        new(value);

    public StudentAwardId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}