﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Covers;
using Constellation.Core.Models.Covers.Identifiers;
using Constellation.Core.Models.Covers.Repositories;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Covers.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class CoverRepository : ICoverRepository
{
    private readonly AppDbContext _context;

    public CoverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cover>> GetAllCurrentAndUpcoming(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Cover>()
            .Where(cover =>
                cover.EndDate >= today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cover>> GetAllCurrent(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Cover>()
            .Where(cover =>
                cover.EndDate >= today &&
                cover.StartDate <= today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cover>> GetAllUpcoming(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Cover>()
            .Where(cover =>
                cover.StartDate > today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cover>> GetAllForCurrentCalendarYear(
        CancellationToken cancellationToken = default)
    {
        var thisYear = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, 1, 1));
        var nextYear = DateOnly.FromDateTime(new DateTime(DateTime.Today.AddYears(1).Year, 1, 1));

        return await _context
            .Set<Cover>()
            .Where(cover => ((cover.StartDate >= thisYear && cover.StartDate <= nextYear ) || (cover.EndDate >= thisYear && cover.EndDate <= nextYear)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Cover?> GetById(
        CoverId CoverId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Cover>()
            .Where(cover => cover.Id == CoverId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<string>> GetCurrentTeacherEmailsForAccessProvisioning(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> returnData = new();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var covers = await _context
            .Set<Cover>()
            .Where(cover => 
                cover.OfferingId == offeringId &&
                cover.EndDate >= today &&
                !cover.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var cover in covers)
        {
            if (cover.TeacherType == CoverTeacherType.Casual)
            {
                CasualId casualId = CasualId.FromValue(Guid.Parse(cover.TeacherId));

                returnData.AddRange(await _context
                    .Set<Casual>()
                    .Where(casual => casual.Id == casualId)
                    .Select(casual => casual.EmailAddress.Email)
                    .ToListAsync(cancellationToken));
            }
            else
            {
                StaffId staffId = StaffId.FromValue(Guid.Parse(cover.TeacherId));

                returnData.AddRange(await _context
                    .Set<StaffMember>()
                    .Where(staff => staff.Id == staffId)
                    .Select(staff => staff.EmailAddress.Email)
                    .ToListAsync(cancellationToken));
            }
        }

        return returnData.Distinct().ToList();
    }

    public async Task<List<Cover>> GetAllWithCasualId(
        CasualId casualId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Cover>()
            .Where(cover => 
                cover.TeacherType == CoverTeacherType.Casual &&
                cover.TeacherId == casualId.Value.ToString())
            .ToListAsync(cancellationToken);

    public async Task<List<Cover>> GetAllForDateAndOfferingId(
        DateOnly coverDate,
        OfferingId offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Cover>()
            .Where(cover => 
                !cover.IsDeleted && 
                coverDate >= cover.StartDate && 
                coverDate <= cover.EndDate && 
                cover.OfferingId == offeringId)
            .ToListAsync(cancellationToken);

    public async Task<List<Cover>> GetCurrentForStaff(
        StaffId staffId,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<Cover>()
            .Where(cover =>
                !cover.IsDeleted &&
                cover.StartDate <= today &&
                cover.EndDate >= today &&
                cover.TeacherType == CoverTeacherType.Staff &&
                cover.TeacherId == staffId.ToString())
            .ToListAsync(cancellationToken);
    }

    public void Insert(Cover cover) =>
        _context.Set<Cover>().Add(cover);
}
