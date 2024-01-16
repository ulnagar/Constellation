namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using System;

public sealed record ConsentId(Guid Value)
{
    public static ConsentId FromValue(Guid value) =>
        new(value);

    public ConsentId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}