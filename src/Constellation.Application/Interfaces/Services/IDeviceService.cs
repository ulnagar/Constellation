using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IDeviceService
    {
        Task<ServiceOperationResult<Device>> CreateDevice(DeviceResource deviceResource);
        Task<ServiceOperationResult<Device>> UpdateDevice(string deviceId, DeviceResource deviceResource);
        Task UpdateDeviceStatus(string serialNumber, Status newStatus, string reasonDescription);

        Task AllocateDevice(string serialNumber, string studentId);
        Task DeallocateDevice(string serialNumber);

        Task AddDeviceNote(string serialNumber, string noteText);

    }
}
