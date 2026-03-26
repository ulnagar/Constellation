namespace Constellation.Core.Models.Messaging.Sms;

using Drafts;
using Enums;
using Identifiers;
using Primitives;
using ValueObjects;

public sealed class SmsMessage : IHasCreatedAt
{
    public SmsMessage()
    {
        Id = new();
    }

    public SmsId Id { get; init; }
    public required string SendingModule { get; set; }
    public string? SmsGlobalId { get; set; } // msgid from SMSGlobal (nullable - not known until sent/received)
    public string? OutgoingId { get; set; } // outgoing_id from delivery receipt

    public required SmsRecipient Sender { get; set; }
    public required SmsRecipient Recipient { get; set; }
    public required string Message { get; set; }

    public MessageDirection Direction { get; set; }
    public MessageStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; } // When we created the record
    public DateTimeOffset? SmsGlobalDate { get; set; } // date field from SMSGlobal
}
