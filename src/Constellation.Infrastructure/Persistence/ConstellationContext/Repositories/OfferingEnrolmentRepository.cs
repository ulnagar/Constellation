namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.OfferingEnrolments;
using Core.Models.OfferingEnrolments.Repositories;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;

public class OfferingEnrolmentRepository : IOfferingEnrolmentRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public OfferingEnrolmentRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<List<OfferingEnrolment>> GetCurrentByStudentId(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OfferingId> currentOfferings = _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id);

        return await _context
            .Set<OfferingEnrolment>()
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                currentOfferings.Contains(enrol.OfferingId))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCurrentCountByStudentId(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OfferingId> currentOfferings = _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id);

        return await _context
            .Set<OfferingEnrolment>()
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                currentOfferings.Contains(enrol.OfferingId))
            .CountAsync(cancellationToken);
    }

    public async Task<List<OfferingEnrolment>> GetCurrentByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<OfferingEnrolment>()
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
                (offering.Sessions.Any(session => !session.IsDeleted) ||
                (offering.StartDate <= _dateTime.Today && offering.EndDate >= _dateTime.Today)))
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<OfferingEnrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains(enrolment.OfferingId))
            .CountAsync(cancellationToken);
    }

    public async Task<List<OfferingEnrolment>> GetCurrentByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default)
    {
        List<OfferingId> offeringIds = await _context
            .Set<Offering>()
            .Where(offering =>
                offering.CourseId == courseId &&
                (offering.Sessions.Any(session => !session.IsDeleted) ||
                (offering.StartDate <= _dateTime.Today && offering.EndDate >= _dateTime.Today)))
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<OfferingEnrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains(enrolment.OfferingId))
            .ToListAsync(cancellationToken);
    }

    public void Insert(OfferingEnrolment offeringEnrolment) =>
        _context.Set<OfferingEnrolment>().Add(offeringEnrolment);
}