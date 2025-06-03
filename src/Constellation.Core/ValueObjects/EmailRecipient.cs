namespace Constellation.Core.ValueObjects;

using Errors;
using Primitives;
using Shared;
using System.Collections.Generic;

public sealed class EmailRecipient : ValueObject
{
    public static readonly EmailRecipient AuroraCollege = new("Aurora College", "auroracoll-h.school@det.nsw.edu.au");
    public static readonly EmailRecipient SupportQueue = new("Aurora College", "support@aurora.nsw.edu.au");
    public static readonly EmailRecipient InfoTechTeam = new("Aurora College IT Support", "auroracollegeitsupport@det.nsw.edu.au");
    public static readonly EmailRecipient NoReply = new("Aurora College", "noreply@aurora.nsw.edu.au");

    private EmailRecipient() {} // Required by EF Core

    private EmailRecipient(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public static Result<EmailRecipient> Create(string name, string email)
    {
        Result<EmailAddress> address = EmailAddress.Create(email);

        if (address.IsFailure)
            return Result.Failure<EmailRecipient>(address.Error);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<EmailRecipient>(DomainErrors.ValueObjects.EmailRecipient.NameEmpty);

        return new EmailRecipient(name, email);
    }

    public static Result<EmailRecipient> Create(Name name, EmailAddress email) =>
        new EmailRecipient(name.DisplayName, email.Email);

    public string Name { get; }
    public string Email { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
    }
}
