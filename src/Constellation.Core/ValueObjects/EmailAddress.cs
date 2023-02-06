namespace Constellation.Core.ValueObjects;

using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class EmailAddress : ValueObject
{
    private EmailAddress(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public static Result<EmailAddress> Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<EmailAddress>(new Error(
                "ValueObjects.EmailAddress.EmailEmpty",
                "Email Address must not be empty"));
        }

        if (!(new EmailAddressAttribute().IsValid(email)))
        {
            return Result.Failure<EmailAddress>(new Error(
                "ValueObjects.EmailAddress.EmailInvalid",
                "Email Address must be valid"));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<EmailAddress>(new Error(
                "ValueObjects.EmailAddress.NameNotDefined",
                "Email Address must have a valid name"));
        }

        return new EmailAddress(name, email);
    }

    public string Name { get; }
    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }
}
