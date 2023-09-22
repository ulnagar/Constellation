namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Abstractions.Clock;
using Microsoft.EntityFrameworkCore;

public class OfferingRepository : IOfferingRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public OfferingRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public void Insert(Offering offering) =>
        _context.Set<Offering>().Add(offering);

    public async Task<Offering?> GetById(
        OfferingId offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Offering>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate <= DateOnly.FromDateTime(DateTime.Today) &&
                offering.EndDate >= DateOnly.FromDateTime(DateTime.Today) &&
                offering.Sessions.Any(session => !session.IsDeleted))
            .OrderBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveForTeacher(
        string StaffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.Teachers.Any(teacher =>
                    teacher.StaffId == StaffId &&
                    !teacher.IsDeleted) &&
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .OrderBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering => offering.CourseId == courseId)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByGrade(
        Grade grade, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Course>()
            .Where(course => course.Grade == grade)
            .SelectMany(course => course.Offerings)
            .Where(offering => offering.IsCurrent)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Include(offering => offering.Sessions)
            .Where(offering => offering.CourseId == courseId && offering.IsCurrent)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
        string studentId,
        DateTime AbsenceDate,
        int DayNumber,
        CancellationToken cancellationToken = default)
    {
        List<int> periodIds = await _context
            .Set<TimetablePeriod>()
            .Where(period => period.Day == DayNumber)
            .Select(period => period.Id)
            .ToListAsync(cancellationToken);

        List<OfferingId> offeringIds = await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                // enrolment was created before the absence date
                enrolment.CreatedAt < AbsenceDate &&
                // enrolment is either still current (not deleted) OR was deleted after the absence date
                (!enrolment.IsDeleted || enrolment.DeletedAt.Date > AbsenceDate))
            .Select(enrolment => enrolment.OfferingId)
            .ToListAsync(cancellationToken);

        List<Offering> offerings = await _context
            .Set<Offering>()
            .Where(offering =>
                offeringIds.Contains(offering.Id) &&
                // offering ends after the absence date
                offering.EndDate > DateOnly.FromDateTime(AbsenceDate) &&
                offering.Sessions.Any(session =>
                    // session was created before the absence date
                    session.CreatedAt < AbsenceDate &&
                    // session is either still current (not deleted) OR was deleted after the absence date
                    (!session.IsDeleted || session.DeletedAt.Date > AbsenceDate) &&
                    // session is for the same day as the absence
                    periodIds.Contains(session.PeriodId)))
            .Distinct()
            .ToListAsync(cancellationToken);

        return offerings;
    }


    // Method is not async as we are passing the task to another method
    public Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
        string studentId,
        DateOnly AbsenceDate,
        int DayNumber,
        CancellationToken cancellationToken = default) =>
        GetCurrentEnrolmentsFromStudentForDate(studentId, AbsenceDate.ToDateTime(TimeOnly.MinValue), DayNumber, cancellationToken);

    public async Task<List<Offering>> GetByStudentId(
        string studentId, 
        CancellationToken cancellationToken = default)
    {
        List<OfferingId> offeringIds = await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                !enrolment.IsDeleted)
            .Select(enrolment => enrolment.OfferingId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Offering>()
            .Where(offering => offeringIds.Contains(offering.Id))
            .ToListAsync(cancellationToken);
    }


    public async Task<Offering> GetFromYearAndName(
        int year, 
        string name, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .SingleOrDefaultAsync(offering => offering.Name == name && offering.EndDate.Year == year, cancellationToken);

    public async Task<List<string>> GetTimetableByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<int> periodIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Sessions)
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<TimetablePeriod>()
            .Where(period => periodIds.Contains(period.Id))
            .Select(period => period.Timetable)
            .ToListAsync(cancellationToken);
    }
}