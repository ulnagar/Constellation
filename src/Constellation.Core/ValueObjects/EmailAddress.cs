namespace Constellation.Core.ValueObjects;

using Errors;
using Helpers;
using Primitives;
using Shared;

public sealed class EmailAddress : ValueObject<EmailAddress, string>, IValueObject<EmailAddress, string>
{
    public static readonly EmailAddress None = new("");

    private EmailAddress() { }

    private EmailAddress(string email)
    {
        Value = email;
    }

    public static Result<EmailAddress> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);

        if (!RegularExpressions.EmailAddress().IsMatch(email))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailInvalid);

        return new EmailAddress(email);
    }

    public string Email => Value;
    
    public static implicit operator string(EmailAddress address) =>
        address.ToString();

    public override string ToString() => Email;

    /// <summary>
    /// Do not use. For EF Core Only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EmailAddress FromValue(string value) => new(value);
}
