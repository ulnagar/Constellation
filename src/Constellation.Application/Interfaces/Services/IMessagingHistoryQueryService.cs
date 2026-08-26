namespace Constellation.Application.Interfaces.Services;

using Core.Shared;
using Domains.Messaging.History.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

public interface IMessagingHistoryQueryService
{
    Task<PaginatedList<CommunicationRecordResponse>> GetRecentHistory(
        string? searchQuery,
        MessagingHistoryDateRange dateRange,
        int sortFilter,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public enum MessagingHistoryDateRange
{
    Last30Days,
    CurrentCalendarYear,
    AllTime
}