namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageDetails;

using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;

public sealed record SentMessageDetail(
    EventId EventId,
    DateTime SentAt,
    string SentBy,
    string Message,
    List<SentMessageDetail.RecipientStatus> Recipient)
{
    public sealed record RecipientStatus(
        MessageType Type,
        string RecipientAddress,
        string RecipientName,
        bool Sent);
}