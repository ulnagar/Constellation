namespace Constellation.Core.Models.Messaging.Sms;

using Enums;
using Identifiers;

public sealed class SmsMessage
{
    public SmsMessage()
    {
        Id = new();
    }

    public SmsId Id { get; init; }
    public required string SendingModule { get; set; }
    public string? SmsGlobalId { get; set; } // msgid from SMSGlobal (nullable - not known until sent/received)
    public string? OutgoingId { get; set; } // outgoing_id from delivery receipt

    public required string From { get; set; }
    public required string To { get; set; }
    public required string Message { get; set; }

    public MessageDirection Direction { get; set; }
    public SmsStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; } // When we created the record
    public DateTimeOffset? SmsGlobalDate { get; set; } // date field from SMSGlobal
    public DateTimeOffset? StatusUpdatedAt { get; set; } // update_time from delivery receipt
}
