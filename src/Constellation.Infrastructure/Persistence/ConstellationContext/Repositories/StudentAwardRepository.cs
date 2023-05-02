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

    public async Task<List<StudentAward>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentAward>()
            .ToListAsync(cancellationToken);

    public async Task<List<StudentAward>> GetByStudentId(
        string studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentAward>()
            .Where(award => award.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<List<StudentAward>> GetFromYear(
        int Year,
        CancellationToken cancellationToken = default)
    {
        var yearStart = new DateTime(Year, 1, 1);
        var yearEnd = new DateTime(Year, 12, 31);

        return await _context
            .Set<StudentAward>()
            .Where(award => 
                award.AwardedOn >= yearStart &&
                award.AwardedOn <= yearEnd)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StudentAward>> GetFromRecentMonths(
        int Months, 
        CancellationToken cancellationToken = default)
    {
        var backDate = DateTime.Today.AddMonths(-Months);
        var startMonth = new DateTime(backDate.Year, backDate.Month, 1);

        return await _context
            .Set<StudentAward>()
            .Where(award =>
                award.AwardedOn >= startMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StudentAward>> GetToRecentCount(
        int Count,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StudentAward>()
            .OrderByDescending(award => award.AwardedOn)
            .Take(Count)
            .ToListAsync(cancellationToken);

    public void Insert(StudentAward studentAward) =>
        _context.Set<StudentAward>().Add(studentAward);
}
