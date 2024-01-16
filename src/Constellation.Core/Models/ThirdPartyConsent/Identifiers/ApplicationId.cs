namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using System;

public sealed record ApplicationId(Guid Value)
{
    public static ApplicationId FromValue(Guid value) =>
        new(value);

    public ApplicationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}