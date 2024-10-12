namespace Constellation.Core.Models.ThirdPartyConsent.Identifiers;

using Constellation.Core.Primitives;
using System;

public record struct ApplicationId(Guid Value)
    : IStronglyTypedId
{
    public static ApplicationId Empty => new(Guid.Empty);

    public static ApplicationId FromValue(Guid value) =>
        new(value);

    public ApplicationId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}