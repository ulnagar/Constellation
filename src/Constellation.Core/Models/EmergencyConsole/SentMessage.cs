namespace Constellation.Core.Models.EmergencyConsole;

using Enums;
using Errors;
using Identifiers;
using Shared;

public sealed class SentMessage
{
    private readonly List<MessageStatus> _statuses = [];

    private SentMessage() { }

    private SentMessage(
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
    public IReadOnlyList<MessageStatus> Statuses => _statuses.AsReadOnly();

    public static Result<SentMessage> Create(
        string message,
        DateTime sentAt,
        string sentBy)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Failure<SentMessage>(SentMessageErrors.MessageBlank);
        }

        return new SentMessage(message, sentAt, sentBy);
    }

    public void AddMessage(
        MessageType type,
        string address,
        string name,
        bool sent)
    {
        MessageStatus status = new(Id, type, address, name, sent);

        _statuses.Add(status);
    }
}