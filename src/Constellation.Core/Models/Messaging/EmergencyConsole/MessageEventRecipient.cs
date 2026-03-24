namespace Constellation.Core.Models.Messaging.EmergencyConsole;

using Enums;
using Identifiers;
using Messaging.Enums;

public sealed class MessageEventRecipient
{
    private MessageEventRecipient() { }

    internal MessageEventRecipient(
        EventId eventId,
        MessageType type,
        string name)
    {
        Id = new();

        EventId = eventId;
        Type = type;
        RecipientAddress = string.Empty;
        RecipientName = name;
        Status = MessageStatus.Pending;
    }

    public MessageId Id { get; private set; }
    public EventId EventId { get; private set; }
    public MessageType Type { get; private set; }
    public string RecipientAddress { get; private set; }
    public string RecipientName { get; private set; }
    public MessageStatus Status { get; private set; }

    public void UpdateRecipient(
        MessageType type,
        string address,
        MessageStatus status)
    {
        Type = type;
        RecipientAddress = address;
        Status = status;
    }
}