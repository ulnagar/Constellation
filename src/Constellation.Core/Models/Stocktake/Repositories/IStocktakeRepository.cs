namespace Constellation.Core.Models.Stocktake.Repositories;

using Identifiers;
using StaffMembers.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IStocktakeRepository
{
    Task<List<StocktakeEvent>> GetAll(CancellationToken cancellationToken = default);
    Task<StocktakeEvent?> GetById(StocktakeEventId eventId, CancellationToken cancellationToken = default);
    Task<StocktakeEvent?> GetByIdWithSightings(StocktakeEventId eventId, CancellationToken cancellationToken = default);
    Task<List<StocktakeEvent>> GetCurrentEvents(CancellationToken cancellationToken = default);
    Task<List<StocktakeSighting>> GetActiveSightingsForSchool(StocktakeEventId stocktakeEventId, string schoolCode, CancellationToken cancellationToken = default);
    Task<List<StocktakeSighting>> GetForStaffMember(StocktakeEventId stocktakeEventId, StaffId staffId, string emailAddress, CancellationToken cancellationToken = default);

    void Insert(StocktakeEvent stocktake);
}
