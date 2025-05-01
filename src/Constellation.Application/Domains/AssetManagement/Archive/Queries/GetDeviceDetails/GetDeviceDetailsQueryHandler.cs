namespace Constellation.Application.Domains.AssetManagement.Archive.Queries.GetDeviceDetails;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetDeviceDetailsQueryHandler
: IQueryHandler<GetDeviceDetailsQuery, DeviceDetailsResponse>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetDeviceDetailsQueryHandler(
        IDeviceRepository deviceRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _deviceRepository = deviceRepository;
        _studentRepository = studentRepository;
        _logger = logger.ForContext<GetDeviceDetailsQuery>();
    }

    public async Task<Result<DeviceDetailsResponse>> Handle(GetDeviceDetailsQuery request, CancellationToken cancellationToken)
    {
        Device device = await _deviceRepository.GetDeviceDetails(request.SerialNumber, cancellationToken);

        if (device is null)
        {
            _logger
                .ForContext(nameof(GetDeviceDetailsQuery), request, true)
                .Warning("Failed to retrieve device details");

            return Result.Failure<DeviceDetailsResponse>(new Error("Equipment.Devices.Archive", "Failed to retrieve device details"));
        }

        List<DeviceDetailsResponse.Allocation> allocations = new();

        foreach (DeviceAllocation allocation in device.Allocations)
        {
            Student student = await _studentRepository.GetById(allocation.StudentId, cancellationToken);

            if (student is null)
                continue;

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            allocations.Add(new(
                allocation.DateAllocated,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                allocation.DateDeleted));
        }

        List<DeviceDetailsResponse.Note> notes = new();

        foreach (DeviceNotes note in device.Notes)
        {
            notes.Add(new(
                note.DateEntered,
                note.Details));
        }

        return new DeviceDetailsResponse(
            device.Make,
            device.Model,
            device.SerialNumber,
            device.Description,
            device.Status,
            device.DateReceived,
            device.DateWarrantyExpires,
            device.DateDisposed,
            allocations,
            notes);
    }
}
