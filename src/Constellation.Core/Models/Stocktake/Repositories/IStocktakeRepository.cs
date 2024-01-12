namespace Constellation.Core.Models.Stocktake.Repositories;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStocktakeRepository
{
    Task<List<StocktakeEvent>> GetCurrentEvents(CancellationToken cancellationToken = default);
    Task<List<StocktakeSighting>> GetActiveSightingsForSchool(Guid stocktakeEventId, string schoolCode, CancellationToken cancellationToken = default);
    Task<StocktakeSighting> GetSightingById(Guid sightingId, CancellationToken cancellationToken = default);

    void Insert(StocktakeSighting sighting);
}
