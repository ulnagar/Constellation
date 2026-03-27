namespace Constellation.Application.Domains.Messaging.History.Models;

using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Enums;
using Core.Primitives;

public sealed record CommunicationRecordResponse(
    IStronglyTypedId Id,
    MessageType Type,
    MessageDirection Direction,
    CommunicationRecordResponse.Sender From,
    List<CommunicationRecordResponse.Recipient> Recipients,
    string Subject,
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