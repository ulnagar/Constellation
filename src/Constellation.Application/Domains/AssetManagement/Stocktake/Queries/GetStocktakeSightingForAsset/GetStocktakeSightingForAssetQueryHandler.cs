namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingForAsset;

using Abstractions.Messaging;
using Core.Models.Assets;
using Core.Models.Assets.Errors;
using Core.Models.Assets.Repositories;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeSightingForAssetQueryHandler
: IQueryHandler<GetStocktakeSightingForAssetQuery, StocktakeSightingForAssetResponse>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetStocktakeSightingForAssetQueryHandler(
        IStocktakeRepository stocktakeRepository,
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _assetRepository = assetRepository;
        _logger = logger
            .ForContext<GetStocktakeSightingForAssetQuery>();
    }

    public async Task<Result<StocktakeSightingForAssetResponse>> Handle(GetStocktakeSightingForAssetQuery request, CancellationToken cancellationToken)
    {
        StocktakeEvent @event = await _stocktakeRepository.GetById(request.EventId, cancellationToken);

        if (@event is null)
        {
            _logger
                .ForContext(nameof(GetStocktakeSightingForAssetQuery), request, true)
                .ForContext(nameof(Error), StocktakeEventErrors.EventNotFound(request.EventId), true)
                .Warning("Failed to retrieve existing sighting for Asset Id");

            return Result.Failure<StocktakeSightingForAssetResponse>(StocktakeEventErrors.EventNotFound(request.EventId));
        }

        List<StocktakeSighting> sightings = @event.Sightings
            .Where(entry => entry.AssetNumber == request.AssetNumber)
            .ToList();

        if (sightings.Count == 0 || sightings.All(entry => entry.IsCancelled))
            return new StocktakeSightingForAssetResponse(false, null, null);

        Asset asset = await _assetRepository.GetByAssetNumber(request.AssetNumber, cancellationToken);

        if (asset is null)
        {
            _logger
                .ForContext(nameof(GetStocktakeSightingForAssetQuery), request, true)
                .ForContext(nameof(Error), AssetErrors.NotFoundByAssetNumber(request.AssetNumber), true)
                .Warning("Failed to retrieve existing sighting for Asset Id");

            return Result.Failure<StocktakeSightingForAssetResponse>(AssetErrors.NotFoundByAssetNumber(request.AssetNumber));
        }

        StocktakeSighting sighting = sightings
            .OrderByDescending(entry => entry.SightedAt)
            .FirstOrDefault(entry => !entry.IsCancelled);

        return new StocktakeSightingForAssetResponse(true, 
            asset.CurrentLocation?.SchoolCode ?? string.Empty,
            sighting!.LocationCode);
    }
}
