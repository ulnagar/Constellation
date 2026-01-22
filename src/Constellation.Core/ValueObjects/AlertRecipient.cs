namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;

public sealed class AlertRecipient : ValueObject
{
    private AlertRecipient() { }

    private AlertRecipient(
        Name name, 
        EmailAddress? emailAddress = null, 
        PhoneNumber? phoneNumber = null)
    {
        Name = name;
        EmailAddress = emailAddress ?? EmailAddress.None;
        PhoneNumber = phoneNumber ?? PhoneNumber.Empty;
    }

    public static AlertRecipient Create(Name name, EmailAddress email) =>
        new(name, email, null);

    public static AlertRecipient Create(Name name, PhoneNumber number) =>
        new(name, null, number);

    public static AlertRecipient Create(Name name, EmailAddress email, PhoneNumber number) =>
        new(name, email, number);

    public Name Name { get; private set; }
    public EmailAddress EmailAddress { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public bool HasEmail => EmailAddress != EmailAddress.None;
    public bool HasPhone => PhoneNumber.IsMobile();

    public Result<EmailRecipient> GetEmailRecipient()
    {
        if (!HasEmail)
            return Result.Failure<EmailRecipient>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);

        return EmailRecipient.Create(Name, EmailAddress!);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
        yield return EmailAddress;
        yield return PhoneNumber;
    }
}