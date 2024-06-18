namespace Constellation.Application.Assets.GetAllocationList;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Assets;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolAllocationListQueryHandler
    : IQueryHandler<GetSchoolAllocationListQuery, List<AllocationListItem>>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetSchoolAllocationListQueryHandler(
        ISchoolRepository schoolRepository,
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _assetRepository = assetRepository;
        _logger = logger.ForContext<GetSchoolAllocationListQuery>();
    }

    public async Task<Result<List<AllocationListItem>>> Handle(GetSchoolAllocationListQuery request, CancellationToken cancellationToken)
    {
        List<AllocationListItem> response = new();

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<Asset> assets = await _assetRepository.GetAllActive(cancellationToken);

        foreach (School school in schools)
        {
            List<Asset> schoolAssets = assets
                .Where(entry => entry.CurrentAllocation?.UserId == school.Code)
                .ToList();

            if (!schoolAssets.Any())
            {
                response.Add(new(
                    school.Code,
                    school.Name,
                    string.Empty,
                    AssetId.Empty,
                    AssetNumber.Empty,
                    null,
                    "No currently assigned assets",
                    null,
                    null,
                    string.Empty,
                    null,
                    null,
                    null));
            }

            foreach (Asset asset in schoolAssets)
            {
                response.Add(new(
                    school.Code,
                    school.Name,
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
        }

        return response;
    }
}