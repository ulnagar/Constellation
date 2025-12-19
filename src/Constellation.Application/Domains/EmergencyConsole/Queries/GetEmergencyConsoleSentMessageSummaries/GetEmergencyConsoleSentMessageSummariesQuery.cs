namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageSummaries;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetEmergencyConsoleSentMessageSummariesQuery()
    :IQuery<List<SentMessageSummary>>;