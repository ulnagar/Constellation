namespace Constellation.Infrastructure.Extensions;

using Core.ValueObjects;
using MimeKit;

public static class EmailRecipientExtensions
{
    public static MailboxAddress ToMailboxAddress(this EmailRecipient recipient) =>
        new(recipient.Email, recipient.Name);
}