using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IDeviceNotesRepository
    {
        DeviceNotes WithDetails(int id);
        DeviceNotes WithFilter(Expression<Func<DeviceNotes, bool>> predicate);
        ICollection<DeviceNotes> All();
        ICollection<DeviceNotes> AllWithFilter(Expression<Func<DeviceNotes, bool>> predicate);
        ICollection<DeviceNotes> AllForDevice(string serialNumber);
    }
}