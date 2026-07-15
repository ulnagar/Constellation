namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System.Reflection;

public sealed class SmsRecipient : ValueObject<SmsRecipient, string>, IValueObject<SmsRecipient, string>
{
    private static readonly Dictionary<string, SmsRecipient> _enumerations = CreateEnumerations();

    public static readonly SmsRecipient AuroraNoReply = new("Aurora - No Reply", "Aurora");
    public static readonly SmsRecipient Aurora = new("Aurora", "0400896896");
    public static readonly SmsRecipient Unknown = new(string.Empty, string.Empty);
    
    private SmsRecipient() { } // Required by EF Core

    private SmsRecipient(string name, string phoneNumber)
    {
        Name = name;
        Value = phoneNumber;
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
    public string Number => Value;

    public static SmsRecipient FromValue(string value) =>
        _enumerations.GetValueOrDefault(value);

    private static Dictionary<string, SmsRecipient> CreateEnumerations()
    {
        Type enumerationType = typeof(SmsRecipient);

        IEnumerable<SmsRecipient> fieldsForType = enumerationType
            .GetProperties(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy)
            .Where(fieldInfo =>
                enumerationType.IsAssignableFrom(fieldInfo.PropertyType))
            .Select(fieldInfo =>
                (SmsRecipient)fieldInfo.GetValue(default)!);

        return fieldsForType.ToDictionary(x => x.Number);
    }
}