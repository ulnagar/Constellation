namespace Constellation.Core.Models.Messaging.Sms.Identifiers;

using Primitives;

public readonly record struct SmsId(Guid Value)
    : IStronglyTypedId
{
    public static SmsId Empty => new(Guid.Empty);

    public static SmsId FromValue(Guid value) =>
        new(value);

    public SmsId()
        : this(Guid.CreateVersion7()) { }

    public override string ToString() =>
        Value.ToString();
}
