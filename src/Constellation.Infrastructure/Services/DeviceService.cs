using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC operations
    public class DeviceService : IDeviceService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddDeviceNote(string serialNumber, string noteText)
        {
            // Validate device
            var device = await _unitOfWork.Devices.ForEditAsync(serialNumber);

            if (device == null)
                return;

            var note = new DeviceNotes()
            {
                DateEntered = DateTime.Now,
                SerialNumber = serialNumber,
                Details = noteText
            };

            _unitOfWork.Add(note);
        }

        public async Task AllocateDevice(string serialNumber, string studentId)
        {
            // Validate entries
            var device = await _unitOfWork.Devices.ForEditAsync(serialNumber);

            if (device == null)
                return;

            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            if (student == null)
                return;

            // Create Allocation
            var allocation = new DeviceAllocation()
            {
                SerialNumber = serialNumber,
                StudentId = studentId
            };

            _unitOfWork.Add(allocation);

            await AddDeviceNote(serialNumber, $"Device assigned to student: {student.DisplayName}");
        }

        public async Task<ServiceOperationResult<Device>> CreateDevice(DeviceResource deviceResource)
        {
            // Set up return entity
            var result = new ServiceOperationResult<Device>();

            // Validate entries
            var checkDevice = await _unitOfWork.Devices.ForEditAsync(deviceResource.SerialNumber);

            if (checkDevice == null)
            {
                // Create new device
                var device = new Device
                {
                    SerialNumber = deviceResource.SerialNumber,
                    Make = deviceResource.Make,
                    Model = deviceResource.Model,
                    Description = deviceResource.Description,
                    DateWarrantyExpires = deviceResource.DateWarrantyExpires,
                    DateReceived = DateTime.Now
                };

                _unitOfWork.Add(device);

                result.Success = true;
                result.Entity = device;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A device with that serial number already exists!");
            }

            return result;
        }

        public async Task DeallocateDevice(string serialNumber)
        {
            // Validate entries
            var device = await _unitOfWork.Devices.ForEditAsync(serialNumber);

            if (device == null)
                return;

            var allocation = device.Allocations.SingleOrDefault(a => a.IsDeleted == false);

            if (allocation == null)
                return;

            await UpdateDeviceStatus(serialNumber, Status.RepairingReturning, $"Device unassigned from student: {allocation.Student.DisplayName}");
            allocation.IsDeleted = true;
            allocation.DateDeleted = DateTime.Now;
        }

        public async Task<ServiceOperationResult<Device>> UpdateDevice(string deviceId, DeviceResource deviceResource)
        {
            // Set up return entity
            var result = new ServiceOperationResult<Device>();

            // Validate entries
            var device = await _unitOfWork.Devices.ForEditAsync(deviceId);

            if (device == null)
            {
                result.Success = false;
                result.Errors.Add($"A device with that serial number could not be found!");
            }
            else
            {
                // Update device
                if (!string.IsNullOrWhiteSpace(deviceResource.Make))
                    device.Make = deviceResource.Make;

                if (!string.IsNullOrWhiteSpace(deviceResource.Model))
                    device.Model = deviceResource.Model;

                if (!string.IsNullOrWhiteSpace(deviceResource.Description))
                    device.Description = deviceResource.Description;

                if (deviceResource.DateWarrantyExpires != null)
                    device.DateWarrantyExpires = deviceResource.DateWarrantyExpires;

                result.Success = true;
                result.Entity = device;
            }

            return result;
        }

        public async Task UpdateDeviceStatus(string serialNumber, Status newStatus, string reasonDescription)
        {
            // Validate entries
            var device = await _unitOfWork.Devices.ForEditAsync(serialNumber);

            if (device == null)
                return;

            if (string.IsNullOrWhiteSpace(reasonDescription))
                return;

            device.Status = newStatus;
            await AddDeviceNote(serialNumber, reasonDescription);
        }
    }
}
