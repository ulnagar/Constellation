using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class DeviceNotesRepository : IDeviceNotesRepository
    {
        private readonly AppDbContext _context;

        public DeviceNotesRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<DeviceNotes> Collection()
        {
            return _context.DeviceNotes
                .Include(n => n.Device)
                    .ThenInclude(device => device.Allocations)
                        .ThenInclude(allocation => allocation.Student)
                            .ThenInclude(student => student.School);
        }

        public DeviceNotes WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public DeviceNotes WithFilter(Expression<Func<DeviceNotes, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<DeviceNotes> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<DeviceNotes> AllWithFilter(Expression<Func<DeviceNotes, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<DeviceNotes> AllForDevice(string serialNumber)
        {
            return Collection()
                .Where(n => n.SerialNumber == serialNumber)
                .ToList();
        }
    }
}