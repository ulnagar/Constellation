namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> GetDeviceById(
        string SerialNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .FirstOrDefaultAsync(device => device.SerialNumber == SerialNumber, cancellationToken);

    public async Task<List<Device>> GetActiveDevicesForStudent(
        string StudentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .Include(device => device.Allocations)
            .Where(device => 
                device.Allocations.Any(allocation => 
                    allocation.StudentId == StudentId && 
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);

    private IQueryable<Device> Collection()
    {
        return _context.Devices
            .Include(d => d.Allocations)
            .ThenInclude(a => a.Student)
            .ThenInclude(s => s.School)
            .Include(d => d.Notes);
    }

    public Device WithDetails(string id)
    {
        return Collection()
            .SingleOrDefault(d => d.SerialNumber == id);
    }

    public Device WithFilter(Expression<Func<Device, bool>> predicate)
    {
        return Collection()
            .FirstOrDefault(predicate);
    }

    public ICollection<Device> All()
    {
        return Collection()
            .ToList();
    }

    public ICollection<Device> AllWithFilter(Expression<Func<Device, bool>> predicate)
    {
        return Collection()
            .Where(predicate)
            .ToList();
    }

    public ICollection<Device> AllActive()
    {
        return _context.Devices
            .Include(d => d.Allocations)
            .ThenInclude(a => a.Student)
            .ThenInclude(s => s.School)
            .Include(d => d.Notes)
            .Where(d => (int)d.Status < 9)
            .OrderBy(d => (int)d.Status)
            .ToList();
    }

    public ICollection<Device> AllInactive()
    {
        return Collection()
            .Where(d => (int)d.Status > 8)
            .OrderBy(d => (int)d.Status)
            .ToList();
    }

    public ICollection<Device> AllForStudent(string studentId)
    {
        return Collection()
            .Where(d => d.Allocations.Any(a => a.StudentId == studentId))
            .ToList();
    }

    public async Task<ICollection<Device>> ForListAsync(Expression<Func<Device, bool>> predicate)
    {
        return await _context.Devices
            .Include(device => device.Allocations)
            .ThenInclude(loan => loan.Student)
            .ThenInclude(student => student.School)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<ICollection<Device>> ForReportingAsync()
    {
        return await _context.Devices
            .Include(device => device.Allocations)
            .ToListAsync();
    }

    public async Task<ICollection<string>> ListOfMakesAsync()
    {
        return await _context.Devices
            .Select(device => device.Make)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Device> ForDetailDisplayAsync(string id)
    {
        return await _context.Devices
            .Include(device => device.Allocations)
            .ThenInclude(loan => loan.Student)
            .ThenInclude(student => student.School)
            .Include(device => device.Notes)
            .SingleOrDefaultAsync(device => device.SerialNumber == id);
    }

    public async Task<Device> ForEditAsync(string id)
    {
        return await _context.Devices
            .SingleOrDefaultAsync(device => device.SerialNumber == id);
    }
}