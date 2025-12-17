namespace Constellation.Core.Models.EmergencyConsole;

using Enums;
using Identifiers;

public sealed class MessageStatus
{
    private MessageStatus() { }

    internal MessageStatus(
        EventId eventId,
        MessageType type,
        string address,
        string name,
        bool sent)
    {
        Id = new();

        EventId = eventId;
        Type = type;
        RecipientAddress = address;
        RecipientName = name;
        Sent = sent;
    }

    public MessageId Id { get; private set; }
    public EventId EventId { get; private set; }
    public MessageType Type { get; private set; }
    public string RecipientAddress { get; private set; }
    public string RecipientName { get; private set; }
    public bool Sent { get; private set; }
}