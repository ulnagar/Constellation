namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;

using Abstractions.Messaging;

public sealed record GetEmergencyConsoleMessageEventSummariesQuery()
    :IQuery<List<MessageEventSummary>>;