namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Awards;
using Microsoft.EntityFrameworkCore;

internal sealed class StudentAwardRepository : IStudentAwardRepository
{
    private readonly AppDbContext _context;

    public StudentAwardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentAward>> GetByStudentId(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentAward>()
            .Where(award => award.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public void Insert(StudentAward studentAward) =>
        _context.Set<StudentAward>().Add(studentAward);
}
