namespace Constellation.Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;

using Abstractions.Messaging;
using Application.Models;
using Core.Shared;
using Interfaces.Services;
using Models;

internal sealed class GetRecentCommunicationsHistoryQueryHandler
: IQueryHandler<GetRecentCommunicationsHistoryQuery, PaginatedList<CommunicationRecordResponse>>
{
    private readonly IMessagingHistoryQueryService _historyQueryService;

    public GetRecentCommunicationsHistoryQueryHandler(
        IMessagingHistoryQueryService historyQueryService)
    {
        _historyQueryService = historyQueryService;
    }

    public async Task<Result<PaginatedList<CommunicationRecordResponse>>> Handle(
        GetRecentCommunicationsHistoryQuery request,
        CancellationToken cancellationToken)
    {
        PaginatedList<CommunicationRecordResponse> queryResponse = await _historyQueryService.GetRecentHistory(
            request.SearchQuery, 
            request.DateRange,
            request.SortColumn,
            request.SortDirection,
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        return queryResponse;
    }
}
