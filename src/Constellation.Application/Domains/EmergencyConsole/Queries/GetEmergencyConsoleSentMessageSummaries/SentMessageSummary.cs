namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageSummaries;

using Core.Models.EmergencyConsole.Identifiers;

public sealed record SentMessageSummary(
    EventId Id,
    string Message,
    int Recipients);