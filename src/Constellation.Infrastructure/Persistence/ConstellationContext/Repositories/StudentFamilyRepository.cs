namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;

internal sealed class StudentFamilyRepository : IStudentFamilyRepository
{
    private readonly AppDbContext _context;

    public StudentFamilyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentFamily>> GetFamilyWithEmail(string email, CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentFamily>()
            .Where(family => family.Parent1.EmailAddress == email || family.Parent2.EmailAddress == email)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetStudentIdsFromFamilyWithEmail(
        string email,
        CancellationToken cancellation = default) =>
        await _context
            .Set<StudentFamily>()
            .Where(family => 
                family.EmailAddress == email ||
                family.Parent1.EmailAddress == email ||
                family.Parent2.EmailAddress == email)
            .SelectMany(family => family.Students)
            .Select(student => student.StudentId)
            .ToListAsync(cancellation);
}
