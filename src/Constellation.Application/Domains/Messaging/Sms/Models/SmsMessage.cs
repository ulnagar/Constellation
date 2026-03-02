namespace Constellation.Application.Domains.Messaging.Sms.Models;

using Enums;
using Identifiers;
using System;
using System.Collections.Generic;

public sealed class SmsMessage
{
    public SmsMessage()
    {
        Id = new();
    }

    public SmsId Id { get; init; }
    public string? SmsGlobalId { get; set; } // msgid from SMSGlobal (nullable - not known until sent/received)
    public string? OutgoingId { get; set; } // outgoing_id from delivery receipt

    public required string From { get; set; }
    public required string To { get; set; }
    public required string Message { get; set; }

    public SmsDirection Direction { get; set; }
    public SmsStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; } // When we created the record
    public DateTimeOffset? SmsGlobalDate { get; set; } // date field from SMSGlobal
    public DateTimeOffset? StatusUpdatedAt { get; set; } // update_time from delivery receipt

    // Self-referencing FK to link a reply to its original message
    public SmsId? ReplyToId { get; set; }
    public SmsMessage? ReplyTo { get; set; }
    public ICollection<SmsMessage> Replies { get; set; } = [];
}
