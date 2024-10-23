using Constellation.Core.Primitives;
using System;

namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public record struct ConsentRequirementId(Guid Value)
    : IStronglyTypedId
{
    public static ConsentRequirementId Empty => new(Guid.Empty);

    public static ConsentRequirementId FromValue(Guid value) =>
        new(value);

    public ConsentRequirementId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}