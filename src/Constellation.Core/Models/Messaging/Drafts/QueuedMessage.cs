namespace Constellation.Core.Models.Messaging.Drafts;

using Constellation.Core.Models.Messaging.Drafts.Enums;
using Constellation.Core.Models.Messaging.Enums;
using Constellation.Core.ValueObjects;
using Identifiers;
using System;
using System.Collections.Generic;

public sealed class QueuedMessage
{
    private readonly List<MessageRecipient> _recipients = [];

    private QueuedMessage() { } // EF

    private QueuedMessage(
        MessageDraft draft,
        MessagePriority priority)
    {
        Id = new QueuedMessageId();
        Type = draft.Type;
        Sender = draft.Sender;
        Subject = draft.Subject;
        Body = draft.Body;
        QueuedAt = DateTimeOffset.UtcNow;
        Priority = priority;

        foreach (MessageRecipient recipient in draft.Recipients)
            _recipients.Add(recipient);
    }
    
    public QueuedMessageId Id { get; private set; }
    public MessageType Type { get; private set; } = MessageType.Email;
    public MessageSender? Sender { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; private set; }
    public MessagePriority Priority { get; private set; } = MessagePriority.Normal;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public IReadOnlyList<MessageRecipient> Recipients => _recipients.AsReadOnly();

    public void MarkProcessed() => ProcessedAt = DateTimeOffset.UtcNow;
    public void MarkFailed(string error) => Error = error;

    public static QueuedMessage FromDraft(MessageDraft draft, MessagePriority priority = MessagePriority.Normal)
        => new(draft, priority);
}