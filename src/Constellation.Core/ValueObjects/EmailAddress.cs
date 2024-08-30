namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public sealed class EmailAddress : ValueObject
{
    private const string _emailRegex = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,6}|[0-9]{1,3})(\]?)$";

    public static readonly EmailAddress None = new("");

    private EmailAddress(string email)
    {
        Email = email;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);

        if (!Regex.IsMatch(email, _emailRegex))
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailInvalid);

        return new EmailAddress(email);
    }

    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }

    public override string ToString() => Email;

    /// <summary>
    /// Do not use. For EF Core Only.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static EmailAddress FromValue(string value) => new(value);
}
