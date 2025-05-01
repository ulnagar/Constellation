namespace Constellation.Application.Domains.AssetManagement.Archive.Queries.GetDevices;

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
                : await _studentRepository.GetById(allocation.StudentId, cancellationToken);

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

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
            {
                response.Add(new(
                    device.SerialNumber,
                    device.Make,
                    device.Status,
                    student.Name.DisplayName,
                    null,
                    string.Empty));

                continue;
            }

            response.Add(new(
                device.SerialNumber,
                device.Make,
                device.Status,
                student.Name.DisplayName,
                enrolment.Grade,
                enrolment.SchoolName));
        }

        return response;
    }
}
