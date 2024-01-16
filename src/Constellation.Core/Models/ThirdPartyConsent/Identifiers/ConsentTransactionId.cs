namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using System;

public sealed record ConsentTransactionId(Guid Value)
{
    public static ConsentId FromValue(Guid value) =>
        new(value);

    public ConsentTransactionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}