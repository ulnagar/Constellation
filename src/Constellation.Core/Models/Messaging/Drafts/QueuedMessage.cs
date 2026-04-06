namespace Constellation.Core.Models.Messaging.Drafts;

using Constellation.Core.Models.Messaging.Drafts.Enums;
using Constellation.Core.Models.Messaging.Enums;
using Constellation.Core.ValueObjects;
using Identifiers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public sealed class QueuedMessage
{
    private readonly List<MessageRecipient> _recipients = [];
    private readonly List<QueuedMessageError> _errors = [];

    private QueuedMessage() { } // EF

    private QueuedMessage(
        MessageDraft draft,
        MessagePriority priority)
    {
        Id = new QueuedMessageId();
        UserId = draft.UserId;
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
    public Guid UserId { get; private set; }
    public MessageType Type { get; private set; } = MessageType.Email;
    public MessageSender? Sender { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTimeOffset QueuedAt { get; private set; }
    public MessagePriority Priority { get; private set; } = MessagePriority.Normal;
    public DateTimeOffset? ProcessedAt { get; private set; }
    public IReadOnlyList<QueuedMessageError> Errors => _errors.AsReadOnly();
    public IReadOnlyList<MessageRecipient> Recipients => _recipients.AsReadOnly();
    public bool HasErrors { get; private set; } = false;
    public void MarkProcessed() => ProcessedAt = DateTimeOffset.UtcNow;

    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
    };

    public string ErrorsJson
    {
        get => JsonConvert.SerializeObject(_errors, _jsonSettings);
        private set
        {
            List<QueuedMessageError>? errors = JsonConvert.DeserializeObject<List<QueuedMessageError>>(value ?? "[]", _jsonSettings);
            _errors.Clear();
            if (errors is not null)
                _errors.AddRange(errors);
        }
    }

    public void AddError(QueuedMessageError error)
    {
        _errors.Add(error);
        HasErrors = true;
    }

    public static QueuedMessage FromDraft(MessageDraft draft, MessagePriority priority = MessagePriority.Normal)
        => new(draft, priority);
}

public abstract class QueuedMessageError
{
    public string Error { get; init; } = string.Empty;
}

public sealed class RecipientError : QueuedMessageError
{
    private RecipientError() { }

    public RecipientError(
        MessageRecipient recipient,
        string error)
    {
        Recipient = recipient;
        Error = error;
    }

    public MessageRecipient Recipient { get; init; }
}

public sealed class ExceptionError : QueuedMessageError
{
    private ExceptionError() { }

    public ExceptionError(
        string error,
        string details)
    {
        Error = error;
        Details = details;
    }

    public string Details { get; init; }

    public static ExceptionError FromException(Exception ex) => new(ex.GetType().Name, ex.Message);
}