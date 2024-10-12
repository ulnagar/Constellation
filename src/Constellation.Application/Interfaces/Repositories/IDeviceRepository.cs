namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IDeviceRepository
{
    Task<List<Device>> GetAll(CancellationToken cancellationToken = default);
    Task<Device?> GetDeviceById(string serialNumber, CancellationToken cancellationToken = default);
    Task<List<Device>> GetActiveDevicesForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<Device?> GetDeviceDetails(string id, CancellationToken cancellationToken = default);
    Task<List<Device>> GetHistoryForStudent(StudentId studentId, CancellationToken cancellationToken = default);
}