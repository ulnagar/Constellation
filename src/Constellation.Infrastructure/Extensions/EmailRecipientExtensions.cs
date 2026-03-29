namespace Constellation.Infrastructure.Extensions;

using Core.ValueObjects;
using MimeKit;

public static class EmailRecipientExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailRecipient recipient) =>
        new(recipient.Name, recipient.Email);
}

public static class EmailSenderExtensions
{
    public static MailboxAddress ToMailboxAddress(this MessageSender sender) =>
        new(sender.Name, sender.Destination);
}