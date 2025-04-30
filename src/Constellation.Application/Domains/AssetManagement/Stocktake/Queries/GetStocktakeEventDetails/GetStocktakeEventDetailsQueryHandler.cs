namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeEventDetails;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Errors;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeEventDetailsQueryHandler
: IQueryHandler<GetStocktakeEventDetailsQuery, StocktakeEventDetailsResponse>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetStocktakeEventDetailsQueryHandler(
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _logger = logger;
    }

    public async Task<Result<StocktakeEventDetailsResponse>> Handle(GetStocktakeEventDetailsQuery request, CancellationToken cancellationToken)
    {
        StocktakeEvent stocktake = await _stocktakeRepository.GetByIdWithSightings(request.Id, cancellationToken);

        if (stocktake is null)
        {
            _logger
                .ForContext(nameof(GetStocktakeEventDetailsQuery), request, true)
                .ForContext(nameof(Error), StocktakeErrors.EventNotFound(request.Id), true)
                .Warning("Failed to retrieve Stocktake Event details");

            return Result.Failure<StocktakeEventDetailsResponse>(StocktakeErrors.EventNotFound(request.Id));
        }

        List<StocktakeEventDetailsResponse.Sighting> sightings = new();

        foreach (StocktakeSighting sighting in stocktake.Sightings)
        {
            sightings.Add(new(
                sighting.Id,
                sighting.AssetNumber,
                sighting.SerialNumber,
                sighting.Description,
                sighting.LocationName,
                sighting.UserName,
                sighting.IsCancelled,
                sighting.SightedBy,
                sighting.SightedAt));
        }

        return new StocktakeEventDetailsResponse(
            stocktake.Id,
            stocktake.Name,
            stocktake.StartDate,
            stocktake.EndDate,
            sightings);
    }
}
