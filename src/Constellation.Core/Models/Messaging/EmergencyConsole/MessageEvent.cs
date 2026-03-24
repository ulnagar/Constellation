namespace Constellation.Core.Models.Messaging.EmergencyConsole;

using Enums;
using Errors;
using Identifiers;
using Messaging.Enums;
using Primitives;
using Shared;
using ValueObjects;

public sealed class MessageEvent : AggregateRoot
{
    private readonly List<MessageEventRecipient> _recipients = [];

    private MessageEvent() { }

    private MessageEvent(
        string message,
        DateTime sentAt,
        string sentBy)
    {
        Id = new();
        Message = message;
        SentAt = sentAt;
        SentBy = sentBy;
    }

    public EventId Id { get; private set; }
    public string Message { get; private set; }
    public DateTime SentAt { get; private set; }
    public string SentBy { get; private set; }
    public IReadOnlyList<MessageEventRecipient> Recipients => _recipients.AsReadOnly();

    public static Result<MessageEvent> Create(
        string message,
        DateTime sentAt,
        string sentBy)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Failure<MessageEvent>(MessageEventErrors.MessageBlank);
        }

        return new MessageEvent(message, sentAt, sentBy);
    }

    public MessageId AddRecipient(
        MessageType type,
        AlertRecipient recipient)
    {
        MessageEventRecipient eventRecipient = new(Id, type, recipient.Name);

        _recipients.Add(eventRecipient);

        return eventRecipient.Id;
    }
}