namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Covers;
using Constellation.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Staff = Constellation.Core.Models.Staff;

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
            .Where(cover => 
                cover.EndDate >= DateOnly.FromDateTime(DateTime.Today) && 
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<ClassCover>> GetAllUpcoming(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => 
                cover.StartDate > DateOnly.FromDateTime(DateTime.Today) && 
                !cover.IsDeleted)
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

    public async Task<List<string>> GetCurrentCoveringTeachersForOffering(
        int offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        var covers = await _context
            .Set<ClassCover>()
            .Where(cover => 
                cover.OfferingId == offeringId &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var cover in covers)
        {
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                returnData.AddRange(await _context
                    .Set<Casual>()
                    .Where(casual => casual.Id == Guid.Parse(cover.TeacherId))
                    .Select(casual => casual.EmailAddress)
                    .ToListAsync(cancellationToken));
            }
            else
            {
                returnData.AddRange(await _context
                    .Set<Staff>()
                    .Where(staff => staff.StaffId == cover.TeacherId)
                    .Select(staff => $"{staff.PortalUsername}@det.nsw.edu.au")
                    .ToListAsync(cancellationToken));
            }
        }

        return returnData.Distinct().ToList();
    }

    public async Task<List<ClassCover>> GetAllWithCasualId(
        int casualId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => 
                cover.TeacherType == CoverTeacherType.Casual &&
                cover.TeacherId == casualId.ToString())
            .ToListAsync(cancellationToken);

    public async Task<List<ClassCover>> GetAllForDateAndOfferingId(
        DateOnly coverDate, 
        int offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => 
                !cover.IsDeleted && 
                coverDate >= cover.StartDate && 
                coverDate <= cover.EndDate && 
                cover.OfferingId == offeringId)
            .ToListAsync(cancellationToken);

    public void Insert(ClassCover cover) =>
        _context.Set<ClassCover>().Add(cover);
}
