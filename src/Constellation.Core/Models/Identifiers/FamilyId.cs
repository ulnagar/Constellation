namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct FamilyId(Guid Value)
    : IStronglyTypedId
{
    public static FamilyId FromValue(Guid value) =>
        new(value);

    public FamilyId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}