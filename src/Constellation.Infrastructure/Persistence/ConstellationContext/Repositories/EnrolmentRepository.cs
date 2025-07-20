namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Enrolments.Identifiers;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Enrolments.Repositories;
using Core.Models.Students.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Identifiers;
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

    public async Task<Enrolment> GetById(
        EnrolmentId enrolmentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .FirstOrDefaultAsync(enrol => enrol.Id == enrolmentId, cancellationToken);

    public async Task<List<Enrolment>> GetCurrent(
        CancellationToken cancellationToken = default)
    {
        IQueryable<OfferingId> currentOfferings = _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id);

        IQueryable<TutorialId> currentTutorials = _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today)
            .Select(tutorial => tutorial.Id);

        return await _context
            .Set<Enrolment>()
            .Where(enrol =>
                !enrol.IsDeleted &&
                (currentOfferings.Contains((enrol as OfferingEnrolment).OfferingId)) ||
                currentTutorials.Contains((enrol as TutorialEnrolment).TutorialId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByStudentId(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OfferingId> currentOfferings = _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .Select(offering => offering.Id);

        IQueryable<TutorialId> currentTutorials = _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today)
            .Select(tutorial => tutorial.Id);

        return await _context
            .Set<Enrolment>()
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                (currentOfferings.Contains((enrol as OfferingEnrolment).OfferingId)) ||
                currentTutorials.Contains((enrol as TutorialEnrolment).TutorialId))
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

        IQueryable<TutorialId> currentTutorials = _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today)
            .Select(tutorial => tutorial.Id);

        return await _context
            .Set<Enrolment>()
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                (currentOfferings.Contains((enrol as OfferingEnrolment).OfferingId)) ||
                currentTutorials.Contains((enrol as TutorialEnrolment).TutorialId))
            .CountAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrol => (enrol as OfferingEnrolment).OfferingId == offeringId && !enrol.IsDeleted)
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
            .Set<Enrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains((enrolment as OfferingEnrolment).OfferingId))
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
                (offering.Sessions.Any(session => !session.IsDeleted) ||
                (offering.StartDate <= _dateTime.Today && offering.EndDate >= _dateTime.Today)))
            .Select(offering => offering.Id)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Enrolment>()
            .Where(enrolment =>
                !enrolment.IsDeleted &&
                offeringIds.Contains((enrolment as OfferingEnrolment).OfferingId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetHistoricalForStudent(
        StudentId studentId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        DateTime startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
        DateTime endDateTime = endDate.ToDateTime(TimeOnly.MinValue);

        return await _context
            .Set<Enrolment>()
            .Where(enrolment => 
                enrolment.StudentId == studentId &&
                enrolment.CreatedAt < endDateTime &&
                (enrolment.DeletedAt == DateTime.MinValue || enrolment.DeletedAt > startDateTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByTutorialId(
        TutorialId tutorialId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrol => (enrol as TutorialEnrolment).TutorialId == tutorialId && !enrol.IsDeleted)
            .ToListAsync(cancellationToken);
    
    public void Insert(Enrolment enrolment) =>
        _context.Set<Enrolment>().Add(enrolment);
}