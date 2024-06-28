namespace Constellation.Core.Models.Identifiers;

using System;

public record struct FamilyId(Guid Value)
{
    public static FamilyId FromValue(Guid value) =>
        new(value);

    public FamilyId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}