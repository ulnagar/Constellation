namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.CountStocktakeItemsOutstanding;

using Abstractions.Messaging;
using Constellation.Core.Models.Stocktake;
using Core.Models.Assets;
using Core.Models.Assets.Repositories;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountStocktakeItemsOutstandingQueryHandler
: IQueryHandler<CountStocktakeItemsOutstandingQuery, double>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public CountStocktakeItemsOutstandingQueryHandler(
        IAssetRepository assetRepository,
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _stocktakeRepository = stocktakeRepository;
        _logger = logger;
    }

    public async Task<Result<double>> Handle(CountStocktakeItemsOutstandingQuery request, CancellationToken cancellationToken)
    {
        List<Asset> activeAssets = await _assetRepository.GetAllActive(cancellationToken);

        List<StocktakeEvent> currentEvents = await _stocktakeRepository.GetCurrentEvents(cancellationToken);

        if (currentEvents.Count == 0)
            return Result.Failure<double>(Error.NullValue);

        double sightedDeviceCount = currentEvents
            .SelectMany(item => item.Sightings)
            .Count(sighting => !sighting.IsCancelled);

        return ((activeAssets.Count - sightedDeviceCount) / activeAssets.Count) * 100;
    }
}
