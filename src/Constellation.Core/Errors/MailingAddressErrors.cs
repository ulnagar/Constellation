namespace Constellation.Core.Errors;

using Shared;

public static class MailingAddressErrors
{
    public static readonly Error TitleEmpty = new(
        "MailingAddress.TitleEmpty",
        "The Title must contain a value");

    public static readonly Error Line1Empty = new(
        "MailingAddress.Line1Empty",
        "The Line 1 must contain a value");

    public static readonly Error TownEmpty = new(
        "MailingAddress.TownEmpty",
        "The Town must contain a value");

    public static readonly Error StateEmpty = new(
        "MailingAddress.StateEmpty",
        "The State must contain a value");

    public static readonly Error PostCodeEmpty = new(
        "MailingAddress.PostCodeEmpty",
        "The PostCode must contain a value");
}