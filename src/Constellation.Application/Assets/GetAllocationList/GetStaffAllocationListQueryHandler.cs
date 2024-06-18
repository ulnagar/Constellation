﻿namespace Constellation.Application.Assets.GetAllocationList;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Assets;
using Core.Models.Assets.Identifiers;
using Core.Models.Assets.Repositories;
using Core.Models.Assets.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffAllocationListQueryHandler
: IQueryHandler<GetStaffAllocationListQuery, List<AllocationListItem>>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly ILogger _logger;

    public GetStaffAllocationListQueryHandler(
        IStaffRepository staffRepository,
        IAssetRepository assetRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _assetRepository = assetRepository;
        _logger = logger.ForContext<GetStaffAllocationListQuery>();
    }

    public async Task<Result<List<AllocationListItem>>> Handle(GetStaffAllocationListQuery request, CancellationToken cancellationToken)
    {
        List<AllocationListItem> response = new();

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<Asset> assets = await _assetRepository.GetAllActive(cancellationToken);

        foreach (Staff staffMember in staffMembers)
        {
            List<Asset> staffAssets = assets
                .Where(entry => entry.CurrentAllocation?.UserId == staffMember.StaffId)
                .ToList();

            if (!staffAssets.Any())
            {
                response.Add(new(
                    staffMember.StaffId,
                    staffMember.GetName()?.DisplayName,
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

            foreach (Asset asset in staffAssets)
            {
                response.Add(new(
                    staffMember.StaffId,
                    staffMember.GetName()?.DisplayName,
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
