using Constellation.Core.Abstractions;
using Constellation.Core.Models.Covers;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

internal sealed class ClassCoverRepository : IClassCoverRepository
{
    private readonly AppDbContext _context;

    public ClassCoverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassCover>> GetAllCurrentAndUpcoming(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => cover.EndDate >= DateOnly.FromDateTime(DateTime.Today) && !cover.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<ClassCover>> GetAllForCurrentCalendarYear(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => (cover.StartDate.Year == DateTime.Today.Year || cover.EndDate.Year == DateTime.Today.Year) && !cover.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<ClassCover?> GetById(
        Guid CoverId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => cover.Id == CoverId)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(ClassCover cover) =>
        _context.Set<ClassCover>().Add(cover);
}
