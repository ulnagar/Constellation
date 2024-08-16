namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using System;

public record struct ConsentTransactionId(Guid Value)
{
    public static ConsentTransactionId Empty => new(Guid.Empty);

    public static ConsentTransactionId FromValue(Guid value) =>
        new(value);

    public ConsentTransactionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}