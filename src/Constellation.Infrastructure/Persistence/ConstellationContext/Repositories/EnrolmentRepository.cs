namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;

public class EnrolmentRepository : IEnrolmentRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public EnrolmentRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<Enrolment>> GetCurrentByStudentId(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        List<OfferingId> currentOfferings = await _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Enrolment>()
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                currentOfferings.Contains(enrol.OfferingId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrol => enrol.OfferingId == offeringId && !enrol.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<int> GetCurrentCountByCourseId(
        CourseId courseId, 
        CancellationToken cancellationToken = default)
    {
        List<OfferingId> offeringIds = await _context
            .Set<Offering>()
            .Where(offering =>
                offering.CourseId == courseId &&
                offering.IsCurrent)
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains(enrolment.OfferingId))
            .CountAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default)
    {
        List<OfferingId> offeringIds = await _context
            .Set<Offering>()
            .Where(offering =>
                offering.CourseId == courseId &&
                offering.IsCurrent)
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains(enrolment.OfferingId))
            .ToListAsync(cancellationToken);
    }

    public void Insert(Enrolment enrolment) =>
        _context.Set<Enrolment>().Add(enrolment);
}