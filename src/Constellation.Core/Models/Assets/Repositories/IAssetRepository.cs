namespace Constellation.Core.Models.Assets.Repositories;

using Enums;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAssetRepository
{
    Task<Asset?> GetById(AssetId assetId, CancellationToken cancellationToken);
    Task<List<Asset>> GetAll(CancellationToken cancellationToken);
    Task<List<Asset>> GetAllActive(CancellationToken cancellationToken);
    Task<List<Asset>> GetAllByStatus(AssetStatus status, CancellationToken cancellationToken);

    void Insert(Asset asset);
    void Insert(Allocation allocation);
    void Insert(Location location);
    void Insert(Sighting sighting);
    void Insert(Note note);
}
