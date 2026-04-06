namespace Constellation.Core.Models.Messaging.Email;

using Enums;
using Identifiers;

public sealed class EmailMessageRecipient
{
    public EmailId EmailId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public EmailRecipientType RecipientType { get; set; }  // To, Cc, Bcc
}