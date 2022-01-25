using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IDeviceAllocationRepository
    {
        DeviceAllocation WithDetails(int id);
        DeviceAllocation WithFilter(Expression<Func<DeviceAllocation, bool>> predicate);
        ICollection<DeviceAllocation> All();
        ICollection<DeviceAllocation> AllWithFilter(Expression<Func<DeviceAllocation, bool>> predicate);
        ICollection<DeviceAllocation> AllForDevice(string serialNumber);
        ICollection<DeviceAllocation> CurrentForDevice(string serialNumber);
    }
}