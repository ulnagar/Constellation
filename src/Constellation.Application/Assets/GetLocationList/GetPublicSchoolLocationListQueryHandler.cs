﻿namespace Constellation.Application.Assets.GetLocationList;

using Abstractions.Messaging;
using Core.Models.Assets;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetLocationListQueryHandler
: IQueryHandler<GetLocationListQuery, List<LocationListItem>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetLocationListQueryHandler(
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _logger = logger;
    }

    public async Task<Result<List<LocationListItem>>> Handle(GetLocationListQuery request, CancellationToken cancellationToken)
    {
        List<LocationListItem> response = new();

        List<Asset> assets = await _assetRepository.GetAllByLocationCategory(request.Category, cancellationToken);

        foreach (Asset asset in assets)
        {
            response.Add(new(
                request.Category, 
                asset.CurrentLocation!.Site,
                asset.Id,
                asset.AssetNumber,
                asset.SerialNumber,
                asset.ModelDescription,
                asset.Status,
                asset.CurrentAllocation?.Id ?? AllocationId.Empty,
                asset.CurrentAllocation?.ResponsibleOfficer ?? string.Empty));
        }

        return response;
    }
}
