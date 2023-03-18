namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetDevicesAllocatedToStudentQueryHandler
    : IQueryHandler<GetDevicesAllocatedToStudentQuery, List<StudentDeviceResponse>>
{
    private readonly IDeviceAllocationRepository _allocationRepository;
    private readonly IDeviceRepository _deviceRepository;

    public GetDevicesAllocatedToStudentQueryHandler(
        IDeviceAllocationRepository allocationRepository,
        IDeviceRepository deviceRepository)
    {
        _allocationRepository = allocationRepository;
        _deviceRepository = deviceRepository;
    }

    public async Task<Result<List<StudentDeviceResponse>>> Handle(GetDevicesAllocatedToStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentDeviceResponse> returnData = new();

        var allocations = await _allocationRepository.GetHistoryForStudent(request.StudentId, cancellationToken);

        if (allocations is null)
        {
            return Result.Failure<List<StudentDeviceResponse>>(DomainErrors.Assets.Allocations.NotFoundForStudent(request.StudentId));
        }

        foreach (var allocation in allocations)
        {
            var device = await _deviceRepository.GetDeviceById(allocation.SerialNumber, cancellationToken);

            if (device is null)
                continue;

            returnData.Add(new(
                device.SerialNumber,
                device.Make,
                device.Status.ToString(),
                DateOnly.FromDateTime(allocation.DateAllocated),
                allocation.DateDeleted.HasValue ? DateOnly.FromDateTime(allocation.DateDeleted.Value) : null));
        }

        return returnData;
    }
}
