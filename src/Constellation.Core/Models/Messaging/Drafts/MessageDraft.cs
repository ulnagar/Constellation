namespace Constellation.Core.Models.Messaging.Drafts;

using Enums;
using System;
using System.Collections.Generic;

public sealed class MessageDraft
{
    private readonly List<MessageRecipient> _recipients = [];

    public MessageDraft(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; init; }
    public MessageType Type { get; set; } = MessageType.Email;
    public IReadOnlyList<MessageRecipient> Recipients => _recipients.AsReadOnly(); 
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }

    public void AddRecipient(MessageRecipient recipient) =>
        _recipients.Add(recipient);
    
    public void RemoveRecipient(MessageRecipient recipient) =>
        _recipients.Remove(recipient);
}