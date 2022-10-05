using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class DeviceAllocationRepository : IDeviceAllocationRepository
    {
        private readonly AppDbContext _context;

        public DeviceAllocationRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<DeviceAllocation> Collection()
        {
            return _context.DeviceAllocations
                .Include(a => a.Device)
                    .ThenInclude(device => device.Notes)
                .Include(a => a.Student)
                    .ThenInclude(student => student.School);
        }

        public DeviceAllocation WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public DeviceAllocation WithFilter(Expression<Func<DeviceAllocation, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<DeviceAllocation> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<DeviceAllocation> AllWithFilter(Expression<Func<DeviceAllocation, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<DeviceAllocation> AllForDevice(string serialNumber)
        {
            return Collection()
                .Where(a => a.SerialNumber == serialNumber)
                .ToList();
        }

        public ICollection<DeviceAllocation> CurrentForDevice(string serialNumber)
        {
            return Collection()
                .Where(a => a.SerialNumber == serialNumber && !a.IsDeleted)
                .ToList();
        }
    }
}