namespace Constellation.Core.Errors;

using Shared;

public static class SmsRecipientErrors
{
    public static readonly Error NameEmpty = new(
        "SMSRecipient.NameEmpty",
        "Email Recipient must have a valid name.");
}