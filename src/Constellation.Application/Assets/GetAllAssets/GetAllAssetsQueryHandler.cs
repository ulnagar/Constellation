#nullable enable
namespace Constellation.Application.Assets.GetAllAssets;

using Abstractions.Messaging;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllAssetsQueryHandler
: IQueryHandler<GetAllAssetsQuery, List<AssetListItem>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetAllAssetsQueryHandler(
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _logger = logger;
    }

    public async Task<Result<List<AssetListItem>>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
    {
        List<AssetListItem> response = new();

        List<Asset> assets = await _assetRepository.GetAll(cancellationToken);

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
