namespace Constellation.Application.Stocktake.GetStocktakeEventList;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeEventListQueryHandler
: IQueryHandler<GetStocktakeEventListQuery, List<StocktakeEventResponse>>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetStocktakeEventListQueryHandler(
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _logger = logger.ForContext<GetStocktakeEventListQuery>();
    }

    public async Task<Result<List<StocktakeEventResponse>>> Handle(GetStocktakeEventListQuery request, CancellationToken cancellationToken)
    {
        List<StocktakeEventResponse> response = new();

        List<StocktakeEvent> events = await _stocktakeRepository.GetAll(cancellationToken);

        foreach (StocktakeEvent stocktake in events)
        {
            response.Add(new(
                stocktake.Id,
                stocktake.Name,
                stocktake.StartDate,
                stocktake.EndDate, 
                stocktake.AcceptLateResponses));
        }

        return response;
    }
}
