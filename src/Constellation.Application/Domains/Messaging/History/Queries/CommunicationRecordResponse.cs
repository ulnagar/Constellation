namespace Constellation.Application.Domains.Messaging.History.Queries;

using Core.Models.EmergencyConsole.Enums;
using Core.Models.Messaging.Sms.Enums;
using Core.Primitives;

public sealed record CommunicationRecordResponse(
    IStronglyTypedId Id,
    MessageType Type,
    MessageDirection Direction,
    string From,
    List<string> To,
    string Subject,
    MessageStatus Status,
    DateTimeOffset Timestamp);