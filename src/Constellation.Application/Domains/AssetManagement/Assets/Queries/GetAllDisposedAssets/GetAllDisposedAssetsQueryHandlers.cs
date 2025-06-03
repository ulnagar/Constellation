#nullable enable
namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllDisposedAssets;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllDisposedAssetsQueryHandlers
: IQueryHandler<GetAllDisposedAssetsQuery, List<AssetListItem>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetAllDisposedAssetsQueryHandlers(
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _logger = logger;
    }

    public async Task<Result<List<AssetListItem>>> Handle(GetAllDisposedAssetsQuery request, CancellationToken cancellationToken)
    {
        List<AssetListItem> response = new();

        List<Asset> assets = await _assetRepository.GetAllByStatus(AssetStatus.Disposed, cancellationToken);

        foreach (Asset asset in assets)
        {
            Allocation? allocation = asset.CurrentAllocation;

            Location? location = asset.CurrentLocation;

            string locationName = location?.Category switch
            {
                _ when location is null => string.Empty,
                _ when location.Category.Equals(LocationCategory.CoordinatingOffice) => $"{location.Room}",
                _ when location.Category.Equals(LocationCategory.PublicSchool) => $"{location.Site}",
                _ => string.Empty
            };

            response.Add(new(
                asset.Id,
                asset.AssetNumber,
                asset.SerialNumber,
                asset.ModelDescription,
                asset.Status,
                allocation?.Id,
                allocation?.ResponsibleOfficer,
                location?.Id,
                location?.Category.Name ?? string.Empty,
                locationName));
        }

        return response;
    }
}
