namespace Constellation.Application.Domains.Messaging.History.Queries;

using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Enums;
using Core.Primitives;

public sealed record CommunicationRecordResponse(
    IStronglyTypedId Id,
    MessageType Type,
    MessageDirection Direction,
    string From,
    List<CommunicationRecordResponse.Recipient> Recipients,
    string Subject,
    MessageStatus Status,
    DateTimeOffset Timestamp)
{
    public sealed record Recipient(
        EmailRecipientType Type,
        string Name,
        string Contact);
}