namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventDetails;

using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;

public sealed record MessageEventDetail(
    EventId EventId,
    DateTime SentAt,
    string SentBy,
    string Message,
    List<MessageEventDetail.RecipientStatus> Recipient)
{
    public sealed record RecipientStatus(
        MessageType Type,
        string RecipientAddress,
        string RecipientName,
        MessageStatus Status);
}