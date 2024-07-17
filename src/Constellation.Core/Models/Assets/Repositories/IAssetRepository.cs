#nullable enable
namespace Constellation.Core.Models.Assets.Repositories;

using Enums;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ValueObjects;

public interface IAssetRepository
{
    Task<Asset?> GetById(AssetId assetId, CancellationToken cancellationToken = default);
    Task<Asset?> GetByAssetNumber(AssetNumber assetNumber, CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAll(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllActiveAllocatedToStudents(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllActiveAllocatedToSchools(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllActiveAllocatedToStaff(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllActiveAllocatedToCommunity(CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllByStatus(AssetStatus status, CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAllByLocationCategory(LocationCategory category, CancellationToken cancellationToken = default);
    Task<bool> IsAssetNumberTaken(AssetNumber assetNumber, CancellationToken cancellationToken = default);

    void Insert(Asset asset);
    void Insert(Allocation allocation);
    void Insert(Location location);
    void Insert(Sighting sighting);
    void Insert(Note note);
}
