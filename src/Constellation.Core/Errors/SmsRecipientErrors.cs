namespace Constellation.Core.Errors;

using Shared;

public static class SmsRecipientErrors
{
    public static readonly Error NameEmpty = new(
        "SMSRecipient.NameEmpty",
        "SMS Recipient must have a valid name.");

    public static readonly Error NumberEmpty = new(
        "SMSRecipient.NumberEmpty",
        "SMS Recipient must have a valid number");
}