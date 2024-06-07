namespace Constellation.Application.Devices.GetDevices;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetDevicesQueryHandler
: IQueryHandler<GetDevicesQuery, List<DeviceSummaryResponse>>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetDevicesQueryHandler(
        IDeviceRepository deviceRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _deviceRepository = deviceRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<Result<List<DeviceSummaryResponse>>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
    {
        List<DeviceSummaryResponse> response = new();

        List<Device> devices = await _deviceRepository.GetAll(cancellationToken);

        foreach (Device device in devices)
        {
            DeviceAllocation allocation = device.Allocations.SingleOrDefault(allocation => !allocation.IsDeleted);

            Student? student = allocation is null
                ? null
                : await _studentRepository.GetWithSchoolById(allocation.StudentId, cancellationToken);

            if (student is null)
            {
                response.Add(new(
                    device.SerialNumber,
                    device.Make,
                    device.Status,
                    string.Empty,
                    null,
                    string.Empty));

                continue;
            }

            response.Add(new(
                device.SerialNumber,
                device.Make,
                device.Status,
                student.GetName().DisplayName,
                student.CurrentGrade,
                student.School.Name));
        }

        return response;
    }
}
