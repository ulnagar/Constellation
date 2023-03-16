namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Families;
using Microsoft.EntityFrameworkCore;

internal sealed class StudentFamilyRepository : IStudentFamilyRepository
{
    private readonly AppDbContext _context;

    public StudentFamilyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Family?> GetFamilyBySentralId(
        string SentralId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .FirstOrDefaultAsync(family => family.SentralId == SentralId, cancellationToken);

    public async Task<Family?> GetFamilyById(
        Guid Id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .FirstOrDefaultAsync(family => family.Id == Id, cancellationToken);

    public async Task<bool> DoesEmailBelongToParentOrFamily(
        string email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .AnyAsync(family => family.Parents.Any(parent => parent.EmailAddress.ToLower() == email.ToLower()), cancellationToken);

    public async Task<List<string>> GetStudentIdsFromFamilyWithEmail(
        string email,
        CancellationToken cancellation = default) =>
        await _context
            .Set<Family>()
            .Where(family => 
                family.FamilyEmail.ToLower() == email.ToLower() ||
                family.Parents.Any(parent => parent.EmailAddress.ToLower() == email.ToLower()))
            .SelectMany(family => family.Students)
            .Select(student => student.StudentId)
            .ToListAsync(cancellation);

    public void Insert(Family family) =>
        _context.Set<Family>().Add(family);
}
