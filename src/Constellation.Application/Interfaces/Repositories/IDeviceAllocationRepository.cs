namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;

public interface IDeviceAllocationRepository
{
    Task<List<DeviceAllocation>> GetHistoryForStudent(string StudentId, CancellationToken cancellationToken = default);
    DeviceAllocation WithDetails(int id);
    DeviceAllocation WithFilter(Expression<Func<DeviceAllocation, bool>> predicate);
    ICollection<DeviceAllocation> All();
    ICollection<DeviceAllocation> AllWithFilter(Expression<Func<DeviceAllocation, bool>> predicate);
    ICollection<DeviceAllocation> AllForDevice(string serialNumber);
    ICollection<DeviceAllocation> CurrentForDevice(string serialNumber);
}