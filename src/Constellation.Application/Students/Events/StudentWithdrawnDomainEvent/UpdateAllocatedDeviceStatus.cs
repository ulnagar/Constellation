namespace Constellation.Application.Students.Events.StudentWithdrawnDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Core.Models.Students.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAllocatedDeviceStatus 
    : IDomainEventHandler<StudentWithdrawnDomainEvent>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAllocatedDeviceStatus(
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;

        _logger = logger.ForContext<StudentWithdrawnDomainEvent>();
    }

    public async Task Handle(StudentWithdrawnDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to update device status for withdrawal of student with id {studentId}", notification.StudentId);

        List<Device> devices = await _deviceRepository.GetActiveDevicesForStudent(notification.StudentId, cancellationToken);
        
        foreach (Device device in devices)
        {
            DeviceAllocation allocation = device
                .Allocations
                .First(allocation =>
                    !allocation.IsDeleted && 
                    allocation.StudentId == notification.StudentId);

            device.Status = Core.Enums.Status.RepairingReturning;

            DeviceNotes note = new()
            {
                DateEntered = DateTime.Now,
                SerialNumber = device.SerialNumber,
                Details = $"Device unassigned from student due to withdrawal: {allocation.Student.Name.DisplayName}"
            };
            device.Notes.Add(note);

            allocation.IsDeleted = true;
            allocation.DateDeleted = DateTime.Now;

            _logger.Information("Updated device {serial} for withdrawal of student with id {studentId}",
                device.SerialNumber, notification.StudentId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}