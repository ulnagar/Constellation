namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEvent;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeEventQueryHandler
: IQueryHandler<GetStocktakeEventQuery, StocktakeEventResponse>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetStocktakeEventQueryHandler(
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _logger = logger.ForContext<GetStocktakeEventQuery>();
    }

    public async Task<Result<StocktakeEventResponse>> Handle(GetStocktakeEventQuery request, CancellationToken cancellationToken)
    {
        StocktakeEvent stocktakeEvent = await _stocktakeRepository.GetById(request.EventId, cancellationToken);

        if (stocktakeEvent is null)
        {
            _logger
                .ForContext(nameof(GetStocktakeEventQuery), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.EventId), true)
                .Warning("Failed to retrieve Stocktake Event");

            return Result.Failure<StocktakeEventResponse>(StocktakeEventErrors.EventNotFound(request.EventId));
        }

        return new StocktakeEventResponse(
            stocktakeEvent.Id,
            stocktakeEvent.Name,
            stocktakeEvent.StartDate,
            stocktakeEvent.EndDate,
            stocktakeEvent.AcceptLateResponses);
    }
}
