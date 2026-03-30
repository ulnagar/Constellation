namespace Constellation.Infrastructure.Extensions;

using Core.Models.Messaging.Email;
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

public static class EmailMessageRecipientExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailMessageRecipient recipient) =>
        new(recipient.Name, recipient.Email);
}