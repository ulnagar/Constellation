namespace Constellation.Core.Errors;

using Shared;

public static class EmailRecipientErrors
{
    public static readonly Error NameEmpty = new(
        "EmailRecipient.NameEmpty",
        "Email Recipient must have a valid name.");
}