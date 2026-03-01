namespace Constellation.Application.Domains.Messaging.Sms.Identifiers;

using Core.Primitives;
using System;

public readonly record struct SmsId(Guid Value)
    : IStronglyTypedId
{
    public static SmsId Empty => new(Guid.Empty);

    public static SmsId FromValue(Guid value) =>
        new(value);

    public SmsId()
        : this(Guid.NewGuid()) { }

    public override string ToString() =>
        Value.ToString();
}
