namespace Constellation.Core.ValueObjects;

using Constellation.Core.Errors;
using Constellation.Core.Primitives;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class EmailRecipient : ValueObject
{
    private EmailRecipient(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public static Result<EmailRecipient> Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<EmailRecipient>(DomainErrors.ValueObjects.EmailAddress.EmailEmpty);
        }

        if (!(new EmailAddressAttribute().IsValid(email)))
        {
            return Result.Failure<EmailRecipient>(DomainErrors.ValueObjects.EmailAddress.EmailInvalid);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<EmailRecipient>(DomainErrors.ValueObjects.EmailRecipient.NameEmpty);
        }

        return new EmailRecipient(name, email);
    }

    public string Name { get; }
    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }
}
