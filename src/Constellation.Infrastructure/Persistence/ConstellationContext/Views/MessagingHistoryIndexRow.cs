namespace Constellation.Infrastructure.Persistence.ConstellationContext.Views;

using Core.Models.Messaging.Enums;
using System;

public class MessagingHistoryIndexRow
{
    public Guid Id { get; set; }
    public string MessageType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Subject { get; set; }
    public string FromName { get; set; }
    public string? FromAddress { get; set; }
    public string? BodyText { get; set; }
    public string? RecipientSearchText { get; set; }

    public MessageType MessageTypeValue =>
        MessageType == "Email"
            ? Core.Models.Messaging.Enums.MessageType.Email
            : Core.Models.Messaging.Enums.MessageType.SMS;

}