namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

internal sealed class ClassCoverRepository : IClassCoverRepository
{
    private readonly AppDbContext _context;

    public ClassCoverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassCover>> GetAllCurrentAndUpcoming(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<ClassCover>()
            .Where(cover =>
                cover.EndDate >= today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);
    }
        

    public async Task<List<ClassCover>> GetAllUpcoming(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<ClassCover>()
            .Where(cover =>
                cover.StartDate > today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ClassCover>> GetAllForCurrentCalendarYear(
        CancellationToken cancellationToken = default)
    {
        var thisYear = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, 1, 1));
        var nextYear = DateOnly.FromDateTime(new DateTime(DateTime.Today.AddYears(1).Year, 1, 1));

        return await _context
            .Set<ClassCover>()
            .Where(cover => ((cover.StartDate >= thisYear && cover.StartDate <= nextYear ) || (cover.EndDate >= thisYear && cover.EndDate <= nextYear)))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassCover?> GetById(
        ClassCoverId CoverId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => cover.Id == CoverId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<string>> GetCurrentCoveringTeachersForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var covers = await _context
            .Set<ClassCover>()
            .Where(cover => 
                cover.OfferingId == offeringId &&
                cover.EndDate >= today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var cover in covers)
        {
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                returnData.AddRange(await _context
                    .Set<Casual>()
                    .Where(casual => casual.Id == new CasualId(Guid.Parse(cover.TeacherId)))
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
        CasualId casualId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassCover>()
            .Where(cover => 
                cover.TeacherType == CoverTeacherType.Casual &&
                cover.TeacherId == casualId.Value.ToString())
            .ToListAsync(cancellationToken);

    public async Task<List<ClassCover>> GetAllForDateAndOfferingId(
        DateOnly coverDate,
        OfferingId offeringId, 
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
