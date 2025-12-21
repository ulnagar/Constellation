namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetEmergencyConsoleMessageEventSummariesQuery()
    :IQuery<List<MessageEventSummary>>;