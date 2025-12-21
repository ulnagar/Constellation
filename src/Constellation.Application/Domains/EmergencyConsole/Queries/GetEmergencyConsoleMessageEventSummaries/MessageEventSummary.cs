namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;

using Core.Models.EmergencyConsole.Identifiers;

public sealed record MessageEventSummary(
    EventId Id,
    string Message,
    DateTime SentAt,
    string SentBy,
    int Recipients);