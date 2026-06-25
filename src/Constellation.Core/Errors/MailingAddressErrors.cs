namespace Constellation.Core.Errors;

using Shared;

public static class MailingAddressErrors
{
    public static readonly Error StreetEmpty = new(
        "ValueObjects.MailingAddress.StreetEmpty",
        "A street address must be provided.");

    public static readonly Error TownEmpty = new(
        "ValueObjects.MailingAddress.TownEmpty",
        "A town or suburb must be provided.");

    public static readonly Error StateEmpty = new(
        "ValueObjects.MailingAddress.StateEmpty",
        "A state must be provided.");

    public static readonly Error StateInvalid = new(
        "ValueObjects.MailingAddress.StateInvalid",
        "The state provided is not a recognised Australian state or territory.");

    public static readonly Error PostcodeEmpty = new(
        "ValueObjects.MailingAddress.PostcodeEmpty",
        "A postcode must be provided.");

    public static readonly Error PostcodeInvalid = new(
        "ValueObjects.MailingAddress.PostcodeInvalid",
        "A postcode must be exactly four digits.");
}