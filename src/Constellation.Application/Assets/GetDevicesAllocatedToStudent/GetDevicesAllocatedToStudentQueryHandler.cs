﻿namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Abstractions.Messaging;
using Core.Models.Assets;
using Core.Models.Assets.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetDevicesAllocatedToStudentQueryHandler
    : IQueryHandler<GetDevicesAllocatedToStudentQuery, List<StudentDeviceResponse>>
{
    private readonly IAssetRepository _assetRepository;
    private readonly IDeviceRepository _deviceRepository;

    public GetDevicesAllocatedToStudentQueryHandler(
        IAssetRepository assetRepository,
        IDeviceRepository deviceRepository)
    {
        _assetRepository = assetRepository;
        _deviceRepository = deviceRepository;
    }

    public async Task<Result<List<StudentDeviceResponse>>> Handle(GetDevicesAllocatedToStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentDeviceResponse> returnData = new();

        List<Asset> assets = await _assetRepository.GetDeviceHistoryForStudent(request.StudentId, cancellationToken);

        foreach (Asset asset in assets)
        {
            List<Allocation> allocations = asset.Allocations
                .Where(allocation => allocation.UserId.Equals(request.StudentId.ToString(), StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            foreach (Allocation allocation in allocations)
            {
                returnData.Add(new(
                    asset.AssetNumber,
                    asset.SerialNumber,
                    asset.ModelDescription,
                    asset.Status.Name,
                    allocation.AllocationDate,
                    allocation.ReturnDate));
            }
        }

        return returnData;
    }
}
