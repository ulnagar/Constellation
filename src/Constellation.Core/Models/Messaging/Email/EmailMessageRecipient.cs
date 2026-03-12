namespace Constellation.Core.Models.Messaging.Email;

using Enums;
using Identifiers;
using ValueObjects;

public class EmailMessageRecipient
{
    public EmailId EmailId { get; set; }
    public EmailRecipientType RecipientType { get; set; }  // To, Cc, Bcc
    public required EmailRecipient Recipient { get; set; }
}