namespace Constellation.Core.Models.Messaging.Sms;

using Enums;
using Events;
using Identifiers;
using Primitives;
using ValueObjects;

public sealed class SmsMessage : AggregateRoot, IHasCreatedAt
{
    public SmsMessage(
        string module,
        string? smsGlobalId,
        SmsRecipient sender,
        SmsRecipient receiver,
        string message,
        MessageDirection direction,
        MessageStatus status,
        DateTimeOffset createdAt)
    {
        Id = new();

        SendingModule = module;
        SmsGlobalId = smsGlobalId;
        Sender = sender;
        Recipient = receiver;
        Message = message;
        Direction = direction;
        Status = status;
        CreatedAt = createdAt;

        if (direction == MessageDirection.Inbound)
            RaiseDomainEvent(new SmsMessageReceivedDomainEvent(new(), Id));
    }

    public SmsId Id { get; init; }
    public string SendingModule { get; private set; }
    public string? SmsGlobalId { get; private set; } // msgid from SMSGlobal (nullable - not known until sent/received)
    public string? OutgoingId { get; set; } // outgoing_id from delivery receipt

    public SmsRecipient Sender { get; private set; }
    public SmsRecipient Recipient { get; private set; }
    public string Message { get; private set; }

    public MessageDirection Direction { get; private set; }
    public MessageStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; } // When we created the record
    public DateTimeOffset? SmsGlobalDate { get; set; } // date field from SMSGlobal
}