namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Device>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .Include(device => device.Allocations)
            .ToListAsync(cancellationToken);

    public async Task<Device?> GetDeviceById(
        string serialNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .FirstOrDefaultAsync(device => device.SerialNumber == serialNumber, cancellationToken);

    public async Task<List<Device>> GetActiveDevicesForStudent(
        StudentId studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .Include(device => device.Allocations)
            .Where(device => 
                device.Allocations.Any(allocation => 
                    allocation.StudentId == studentId && 
                    !allocation.IsDeleted))
            .ToListAsync(cancellationToken);
    
    public async Task<Device> GetDeviceDetails(
        string id, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .Include(device => device.Allocations)
            .Include(device => device.Notes)
            .SingleOrDefaultAsync(device => device.SerialNumber == id, cancellationToken);

    public async Task<List<Device>> GetHistoryForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Device>()
            .Where(device =>
                device.Allocations.Any(allocation => allocation.StudentId == studentId))
            .ToListAsync(cancellationToken);
}