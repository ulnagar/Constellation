namespace Constellation.Application.Stocktake.GetCurrentStocktakeEvents;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class GetCurrentStocktakeEventsQueryHandler 
    : IQueryHandler<GetCurrentStocktakeEventsQuery, List<StocktakeEventResponse>>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetCurrentStocktakeEventsQueryHandler(
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _logger = logger;
    }

    public async Task<Result<List<StocktakeEventResponse>>> Handle(GetCurrentStocktakeEventsQuery request, CancellationToken cancellationToken)
    {
        List<StocktakeEventResponse> response = new();

        List<StocktakeEvent> currentEvents = await _stocktakeRepository.GetCurrentEvents(cancellationToken);

        foreach (StocktakeEvent item in currentEvents)
        {
            response.Add(new(
                item.Id,
                item.Name,
                item.StartDate,
                item.EndDate));
        }

        return response;
    }
}