namespace Constellation.Application.Domains.Messaging.History.Queries.GetMessageDetails;

using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Enums;
using Core.Primitives;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

public sealed record MessageDetailResponse(
    IStronglyTypedId Id,
    MessageType Type,
    MessageDetailResponse.Sender From,
    List<MessageDetailResponse.Recipient> Recipients,
    string Subject,
    string Body,
    MessageStatus Status,
    DateTimeOffset Timestamp)
{
    public sealed record Sender(
        string Name,
        string Contact);

    public sealed record Recipient(
        EmailRecipientType Type,
        string Name,
        string Contact);
}