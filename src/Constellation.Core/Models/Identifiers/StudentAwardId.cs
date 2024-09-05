namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct StudentAwardId(Guid Value)
    : IStronglyTypedId
{
    public static StudentAwardId Empty => new(Guid.Empty);

    public static StudentAwardId FromValue(Guid value) =>
        new(value);

    public StudentAwardId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}