namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct ConsentId(Guid Value)
    : IStronglyTypedId
{
    public static ConsentId Empty => new(Guid.Empty);

    public static ConsentId FromValue(Guid value) =>
        new(value);

    public ConsentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}