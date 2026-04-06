namespace Constellation.Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetRecentCommunicationsHistoryQuery(
    int Limit = 100)
    : IQuery<List<CommunicationRecordResponse>>;
