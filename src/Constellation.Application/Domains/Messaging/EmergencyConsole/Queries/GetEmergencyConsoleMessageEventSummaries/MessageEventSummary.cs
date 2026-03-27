namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;

using Core.Models.Messaging.EmergencyConsole.Identifiers;

public sealed record MessageEventSummary(
    EventId Id,
    string Message,
    DateTime SentAt,
    string SentBy,
    int Recipients);