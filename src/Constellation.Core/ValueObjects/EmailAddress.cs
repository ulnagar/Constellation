namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class EmailAddress : ValueObject
{
    private EmailAddress(string email)
    {
        Email = email;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);
        }

        if (!(new EmailAddressAttribute().IsValid(email)))
        {
            return Result.Failure<EmailAddress>(DomainErrors.ValueObjects.EmailAddress.EmailInvalid);
        }

        return new EmailAddress(email);
    }

    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }

    public override string ToString()
    {
        return Email;
    }
}
