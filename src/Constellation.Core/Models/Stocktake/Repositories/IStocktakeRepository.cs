namespace Constellation.Core.Models.Stocktake.Repositories;

using StaffMembers.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStocktakeRepository
{
    Task<List<StocktakeEvent>> GetAll(CancellationToken cancellationToken = default);
    Task<StocktakeEvent?> GetById(Guid eventId, CancellationToken cancellationToken = default);
    Task<StocktakeEvent?> GetByIdWithSightings(Guid eventId, CancellationToken cancellationToken = default);
    Task<List<StocktakeEvent>> GetCurrentEvents(CancellationToken cancellationToken = default);
    Task<List<StocktakeSighting>> GetActiveSightingsForSchool(Guid stocktakeEventId, string schoolCode, CancellationToken cancellationToken = default);
    Task<StocktakeSighting?> GetSightingById(Guid sightingId, CancellationToken cancellationToken = default);
    Task<List<StocktakeSighting>> GetForStaffMember(Guid stocktakeEventId, StaffId staffId, string emailAddress, CancellationToken cancellationToken = default);

    void Insert(StocktakeSighting sighting);
    void Insert(StocktakeEvent stocktake);
}
