namespace Constellation.Application.Stocktake.GetStocktakeSightingsForSchool;

using Abstractions.Messaging;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStocktakeSightingsForSchoolQueryHandler 
    : IQueryHandler<GetStocktakeSightingsForSchoolQuery, List<StocktakeSightingResponse>>
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ILogger _logger;

    public GetStocktakeSightingsForSchoolQueryHandler(
        IStocktakeRepository stocktakeRepository,
        ILogger logger)
    {
        _stocktakeRepository = stocktakeRepository;
        _logger = logger.ForContext<GetStocktakeSightingsForSchoolQuery>();
    }

    public async Task<Result<List<StocktakeSightingResponse>>> Handle(GetStocktakeSightingsForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<StocktakeSightingResponse> response = new();

        List<StocktakeSighting> sightings = await _stocktakeRepository.GetActiveSightingsForSchool(
                request.StocktakeEventId, 
                request.SchoolCode,
                cancellationToken);

        foreach (StocktakeSighting sighting in sightings)
        {
            response.Add(new(
                sighting.Id,
                sighting.SerialNumber,
                sighting.AssetNumber,
                sighting.Description,
                sighting.LocationName,
                sighting.UserName,
                sighting.SightedBy,
                sighting.SightedAt));
        }

        return response;
    }
}