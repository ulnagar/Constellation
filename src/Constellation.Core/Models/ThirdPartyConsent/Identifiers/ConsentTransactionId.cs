namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct ConsentTransactionId(Guid Value)
    : IStronglyTypedId
{
    public static ConsentTransactionId Empty => new(Guid.Empty);

    public static ConsentTransactionId FromValue(Guid value) =>
        new(value);

    public ConsentTransactionId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}