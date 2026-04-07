namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class EmailAddress : ValueObject
{
    public static readonly EmailAddress None = new("");

    private EmailAddress() { }
    private EmailAddress(string email)
    {
        Email = email;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);

        if (!RegularExpressions.EmailAddress().IsMatch(email))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailInvalid);

        return new EmailAddress(email);
    }

    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }

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
