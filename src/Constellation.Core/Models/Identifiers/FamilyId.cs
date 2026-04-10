namespace Constellation.Core.Models.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct FamilyId(Guid Value)
    : IStronglyTypedId<FamilyId, Guid>
{
    public static FamilyId Empty => new(Guid.Empty);

    public static FamilyId FromValue(Guid value) =>
        new(value);

    public FamilyId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}