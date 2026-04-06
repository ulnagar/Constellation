namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;

public sealed class SmsRecipient : ValueObject
{
    public static readonly SmsRecipient AuroraNoReply = new("Aurora - No Reply", "Aurora");
    public static readonly SmsRecipient Aurora = new("Aurora", "0400896896");
    public static readonly SmsRecipient Unknown = new(string.Empty, string.Empty);
    
    private SmsRecipient() { } // Required by EF Core

    private SmsRecipient(string name, string phoneNumber)
    {
        Name = name;
        Number = phoneNumber;
    }

    public static Result<SmsRecipient> Create(string name, string phoneNumber)
    {
        Result<PhoneNumber> number = PhoneNumber.Create(phoneNumber);

        if (number.IsFailure)
            return Result.Failure<SmsRecipient>(number.Error);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<SmsRecipient>(SmsRecipientErrors.NameEmpty);

        return new SmsRecipient(name, number.Value.ToString(PhoneNumber.Format.None));
    }

    public static Result<SmsRecipient> Create(Name name, PhoneNumber phoneNumber) =>
        new SmsRecipient(name.DisplayName, phoneNumber.ToString(PhoneNumber.Format.None));

    public string Name { get; private set; }
    public string Number { get; private set; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Number;
    }
}