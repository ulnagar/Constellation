#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class FacultyRepository : IFacultyRepository
{
    private readonly AppDbContext _dbContext;

    public FacultyRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Faculty>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .ToListAsync(cancellationToken);

    public async Task<Faculty> GetById(
        FacultyId facultyId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .FirstOrDefaultAsync(faculty => faculty.Id == facultyId, cancellationToken);

    public async Task<List<Faculty>> GetCurrentForStaffMember(
        StaffId staffId,
        CancellationToken cancellationToken = default) =>
        await _dbContext
            .Set<Faculty>()
            .Include(faculty => faculty.Members)
            .Where(faculty => faculty.Members.Any(member => !member.IsDeleted && member.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<Faculty> GetByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default)
    {
        FacultyId? facultyId = await _dbContext
            .Set<Course>()
            .Where(course => course.Id == courseId)
            .Select(course => course.FacultyId)
            .FirstOrDefaultAsync(cancellationToken);

        return (facultyId is null)
            ? null
            : await _dbContext
                .Set<Faculty>()
                .Where(faculty => faculty.Id == facultyId)
                .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Faculty?> GetByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        CourseId courseId = await _dbContext
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .Select(offering => offering.CourseId)
            .FirstOrDefaultAsync(cancellationToken);

        FacultyId? facultyId = await _dbContext
            .Set<Course>()
            .Where(course => course.Id == courseId)
            .Select(course => course.FacultyId)
            .FirstOrDefaultAsync(cancellationToken);

        return (facultyId is null)
            ? null
            : await _dbContext
            .Set<Faculty>()
            .Where(faculty => faculty.Id == facultyId)
            .FirstOrDefaultAsync(cancellationToken);
    }
        
    public async Task<List<Faculty>> GetListFromIds(
        List<FacultyId> facultyIds,
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
