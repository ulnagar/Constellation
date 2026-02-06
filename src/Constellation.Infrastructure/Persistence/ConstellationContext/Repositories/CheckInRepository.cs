namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Enums;
using Core.Models.Attendance.Checkin;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class CheckInRepository : ICheckInRepository
{
    private readonly AppDbContext _context;

    public CheckInRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CheckInResponse>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .ToListAsync(cancellationToken);

    public async Task<List<CheckInResponse>> GetFromGrade(
        Grade grade,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .Where(response => response.Grade == grade)
            .ToListAsync(cancellationToken);

    public async Task<List<CheckInResponse>> GetFromCourse(
        CourseId courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .Where(response => response.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task<List<CheckInResponse>> GetFromOffering(
        OfferingId offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .Where(response => response.OfferingId == offeringId)
            .ToListAsync(cancellationToken);
    
    public async Task<List<CheckInResponse>> GetFromSchool(
        string schoolCode, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .Where(response => response.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetSentimentList(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<CheckInResponse>()
            .Select(response => response.Sentiment)
            .Distinct()
            .OrderBy(entry => entry)
            .ToListAsync(cancellationToken);

    public void Insert(CheckInResponse item) => _context.Set<CheckInResponse>().Add(item);
}
