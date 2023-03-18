namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Microsoft.EntityFrameworkCore;

internal sealed class ParentRepository : IParentRepository
{
    private readonly AppDbContext _dbContext;

    public ParentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Parent>> GetAllParentsOfActiveStudents(CancellationToken cancellationToken = default)
    {
        var familyIds = await _dbContext
            .Set<Student>()
            .AsNoTrackingWithIdentityResolution()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.FamilyMemberships)
            .Select(member => member.FamilyId)
            .ToListAsync(cancellationToken);

        return await _dbContext
            .Set<Parent>()
            .AsNoTrackingWithIdentityResolution()
            .Where(parent => familyIds.Contains(parent.FamilyId))
            .ToListAsync(cancellationToken);
    }
}
