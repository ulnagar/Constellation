namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using Constellation.Core.Primitives;
using System;

public sealed record ConsentId(Guid Value)
    : IStronglyTypedId
{
    public static ConsentId FromValue(Guid value) =>
        new(value);

    public ConsentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}