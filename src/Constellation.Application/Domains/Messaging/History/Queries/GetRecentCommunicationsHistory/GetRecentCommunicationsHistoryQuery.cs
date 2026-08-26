namespace Constellation.Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;

using Abstractions.Messaging;
using Application.Models;
using Interfaces.Services;
using Models;
using System.Collections.Generic;

public sealed record GetRecentCommunicationsHistoryQuery(
    string? SearchQuery,
    MessagingHistoryDateRange DateRange,
    int SortColumn,
    string SortDirection,
    int PageNumber = 1,
    int PageSize = 50)
    : IQuery<PaginatedList<CommunicationRecordResponse>>;
