namespace Constellation.Core.Models.EmergencyConsole;

using Enums;
using Identifiers;
using Shared;

public sealed class SentMessage
{
    private SentMessage() { }

    private SentMessage(
        EventId eventId,
        MessageType type,
        string address,
        string name,
        string message)
    {
        Id = new();

        EventId = eventId;
        Type = type;
        RecipientAddress = address;
        RecipientName = name;
        Message = message;
    }

    public MessageId Id { get; private set; }
    public EventId EventId { get; private set; }
    public MessageType Type { get; private set; }
    public string RecipientAddress { get; private set; }
    public string RecipientName { get; private set; }
    public string Message { get; private set; }
    public bool Sent { get; private set; }

    public static Result<SentMessage> Create(
        EventId eventId,
        MessageType type,
        string address,
        string name,
        string message)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return Result.Failure<SentMessage>();
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<SentMessage>();
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Result.Failure<SentMessage>();
        }

        return new SentMessage(eventId, type, address, name, message);
    }
}