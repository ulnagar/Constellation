namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class FacultyRepository : IFacultyRepository
{
    private readonly AppDbContext _dbContext;

    public FacultyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Faculty> GetById(
        Guid FacultyId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .FirstOrDefaultAsync(faculty => faculty.Id == FacultyId, cancellationToken);

    public async Task<List<Faculty>> GetCurrentForStaffMember(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .Include(faculty => faculty.Members)
            .ThenInclude(member => member.Staff)
            .Where(faculty => faculty.Members.Any(member => !member.IsDeleted && member.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<Faculty?> GetByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        Guid? facultyId = await _dbContext
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .Select(offering => offering.Course.FacultyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (facultyId is null)
        {
            return null;
        }

        return await _dbContext
            .Set<Faculty>()
            .Where(faculty => faculty.Id == facultyId)
            .FirstOrDefaultAsync(cancellationToken);
    }
        
    public async Task<List<Faculty>> GetListFromIds(
        List<Guid> facultyIds,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .Where(faculty => facultyIds.Contains(faculty.Id))
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsWithName(string name, CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .AnyAsync(faculty => !faculty.IsDeleted && faculty.Name == name, cancellationToken);

    public void Insert(Faculty faculty) =>
        _dbContext
            .Set<Faculty>()
            .Add(faculty);
}
