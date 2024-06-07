namespace Constellation.Application.Assets.GetDevicesAllocatedToStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetDevicesAllocatedToStudentQueryHandler
    : IQueryHandler<GetDevicesAllocatedToStudentQuery, List<StudentDeviceResponse>>
{
    private readonly IDeviceRepository _deviceRepository;

    public GetDevicesAllocatedToStudentQueryHandler(
        IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task<Result<List<StudentDeviceResponse>>> Handle(GetDevicesAllocatedToStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentDeviceResponse> returnData = new();

        List<Device> devices = await _deviceRepository.GetHistoryForStudent(request.StudentId, cancellationToken);

        if (devices is null)
        {
            return Result.Failure<List<StudentDeviceResponse>>(DomainErrors.Assets.Allocations.NotFoundForStudent(request.StudentId));
        }

        List<DeviceAllocation> allocations = devices
            .SelectMany(device =>
                device.Allocations.Where(allocation => 
                    allocation.StudentId == request.StudentId))
            .ToList();

        foreach (DeviceAllocation allocation in allocations)
        {
            returnData.Add(new(
                allocation.Device.SerialNumber,
                allocation.Device.Make,
                allocation.Device.Status.ToString(),
                DateOnly.FromDateTime(allocation.DateAllocated),
                allocation.DateDeleted.HasValue ? DateOnly.FromDateTime(allocation.DateDeleted.Value) : null));
        }

        return returnData;
    }
}
