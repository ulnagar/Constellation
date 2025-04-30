namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllocationList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets;
using Constellation.Core.Models.Assets.Repositories;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCommunityMemberAllocationListQueryHandler
    : IQueryHandler<GetCommunityMemberAllocationListQuery, List<AllocationListItem>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetCommunityMemberAllocationListQueryHandler(
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _assetRepository = assetRepository;
        _logger = logger.ForContext<GetCommunityMemberAllocationListQuery>();
    }

    public async Task<Result<List<AllocationListItem>>> Handle(GetCommunityMemberAllocationListQuery request, CancellationToken cancellationToken)
    {
        List<AllocationListItem> response = new();

        List<Asset> assets = await _assetRepository.GetAllActiveAllocatedToCommunity(cancellationToken);

        foreach (Asset asset in assets)
        {
            response.Add(new(
                asset.CurrentAllocation!.UserId,
                asset.CurrentAllocation!.ResponsibleOfficer,
                string.Empty,
                asset.Id,
                asset.AssetNumber,
                asset.SerialNumber,
                asset.ModelDescription,
                asset.Status,
                asset.CurrentAllocation!.Id,
                string.Empty,
                asset.CurrentLocation?.Id,
                asset.CurrentLocation?.Category.Name,
                asset.CurrentLocation?.Site));
        }

        return response;
    }
}