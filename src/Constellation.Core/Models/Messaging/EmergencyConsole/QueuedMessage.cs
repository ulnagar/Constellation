namespace Constellation.Core.Models.Messaging.EmergencyConsole;

using Identifiers;
using ValueObjects;

public sealed class QueuedMessage
{
    private QueuedMessage() { }
    public QueuedMessage(
        EventId eventId,
        MessageId messageId,
        AlertRecipient alertRecipient)
    {
        EventId = eventId;
        MessageId = messageId;
        AlertRecipient = alertRecipient;
    }

    public EventId EventId { get; private set; }
    public MessageId MessageId { get; private set; }
    public AlertRecipient AlertRecipient { get; private set; }
}
