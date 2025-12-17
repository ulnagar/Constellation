namespace Constellation.Core.Models.EmergencyConsole;

using Enums;
using Errors;
using Identifiers;
using Shared;

public sealed class SentMessage
{
    private List<MessageStatus> _statuses = [];

    private SentMessage() { }

    private SentMessage(
        string message)
    {
        Id = new();
        Message = message;
    }

    public EventId Id { get; private set; }
    public string Message { get; private set; }
    public IReadOnlyList<MessageStatus> Statuses => _statuses.AsReadOnly();

    public static Result<SentMessage> Create(
        string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Failure<SentMessage>(SentMessageErrors.MessageBlank);
        }

        return new SentMessage(message);
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