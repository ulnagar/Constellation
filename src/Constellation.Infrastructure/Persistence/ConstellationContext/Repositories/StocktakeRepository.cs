namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Abstractions.Clock;
using Core.Models.Stocktake;
using Core.Models.Stocktake.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

internal sealed class StocktakeRepository : IStocktakeRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public StocktakeRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<StocktakeEvent>> GetCurrentEvents(
        CancellationToken cancellationToken = default) => 
        await _context
            .Set<StocktakeEvent>()
            .Where(stocktake => 
                stocktake.EndDate.Date >= _dateTime.Now.Date &&
                stocktake.StartDate.Date <= _dateTime.Now.Date)
            .ToListAsync(cancellationToken);

    public async Task<List<StocktakeSighting>> GetActiveSightingsForSchool(
        Guid stocktakeEventId, 
        string schoolCode, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StocktakeSighting>()
            .Where(sighting =>
                sighting.StocktakeEventId == stocktakeEventId &&
                sighting.LocationCode == schoolCode &&
                !sighting.IsCancelled)
            .ToListAsync(cancellationToken);

    public async Task<StocktakeSighting> GetSightingById(
        Guid sightingId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StocktakeSighting>()
            .FirstOrDefaultAsync(sighting => sighting.Id == sightingId, cancellationToken);

    public void Insert(StocktakeSighting sighting) => _context.Set<StocktakeSighting>().Add(sighting);
}