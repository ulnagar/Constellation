namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Canvas.Models;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Abstractions.Clock;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Students.Identifiers;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Threading;

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

    public void Remove(Resource resource) =>
        _context.Set<Resource>().Remove(resource);

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
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today &&
                offering.Sessions.Any(session => !session.IsDeleted))
            .OrderBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetAllInactive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.EndDate < _dateTime.Today)
            .OrderBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetAllFuture(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.StartDate > _dateTime.Today)
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
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByGrade(
        Grade grade, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Course>()
            .Where(course => course.Grade == grade)
            .SelectMany(course => course.Offerings)
            .Where(offering => 
                offering.StartDate <= _dateTime.Today && 
                offering.EndDate >= _dateTime.Today)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByCourseId(
        CourseId courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Include(offering => offering.Sessions)
            .Where(offering => 
                offering.CourseId == courseId &&
                offering.StartDate <= _dateTime.Today && 
                offering.EndDate >= _dateTime.Today)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
        StudentId studentId,
        DateTime absenceDate,
        int dayNumber,
        CancellationToken cancellationToken = default)
    {
        DateOnly absenceDateOnly = DateOnly.FromDateTime(absenceDate);

        List<PeriodId> periodIds = await _context
            .Set<Period>()
            .Where(period => period.DayNumber == dayNumber)
            .Select(period => period.Id)
            .ToListAsync(cancellationToken);

        List<OfferingId> offeringIds = await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                // enrolment was created before the absence date
                enrolment.CreatedAt < absenceDate &&
                // enrolment is either still current (not deleted) OR was deleted after the absence date
                (!enrolment.IsDeleted || enrolment.DeletedAt.Date > absenceDate))
            .Select(enrolment => enrolment.OfferingId)
            .ToListAsync(cancellationToken);

        List<Offering> offerings = await _context
            .Set<Offering>()
            .Where(offering =>
                offeringIds.Contains(offering.Id) &&
                // offering ends after the absence date
                offering.EndDate > absenceDateOnly &&
                offering.Sessions.Any(session =>
                    // session was created before the absence date
                    session.CreatedAt < absenceDate &&
                    // session is either still current (not deleted) OR was deleted after the absence date
                    (!session.IsDeleted || session.DeletedAt.Date > absenceDate) &&
                    // session is for the same day as the absence
                    periodIds.Contains(session.PeriodId)))
            .Distinct()
            .ToListAsync(cancellationToken);

        return offerings;
    }


    // Method is not async as we are passing the task to another method
    public Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
        StudentId studentId,
        DateOnly absenceDate,
        int dayNumber,
        CancellationToken cancellationToken = default) =>
        GetCurrentEnrolmentsFromStudentForDate(studentId, absenceDate.ToDateTime(TimeOnly.MinValue), dayNumber, cancellationToken);

    public async Task<List<Offering>> GetByStudentId(
        StudentId studentId, 
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
            .Where(offering => 
                offeringIds.Contains(offering.Id) &&
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today)
        .ToListAsync(cancellationToken);
    }

    public async Task<List<Offering>> GetWithLinkedTeamResource(
        string teamName, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.Sessions.Any(session => !session.IsDeleted) &&
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today &&
                offering.Resources.Any(resource =>
                    resource.Type == ResourceType.MicrosoftTeam &&
                    resource.ResourceId == teamName))
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetWithLinkedCanvasResource(
        CanvasCourseCode courseCode, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.Sessions.Any(session => !session.IsDeleted) &&
                offering.StartDate <= _dateTime.Today &&
                offering.EndDate >= _dateTime.Today &&
                offering.Resources.Any(resource =>
                    resource.Type == ResourceType.CanvasCourse &&
                    resource.ResourceId == courseCode.ToString()))
            .ToListAsync(cancellationToken);

    public async Task<Offering> GetFromYearAndName(
        int year, 
        string name, 
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = new DateOnly(year, 1, 1);
        DateOnly endOfYear = new DateOnly(year, 12, 31);

        return await _context
            .Set<Offering>()
            .SingleOrDefaultAsync(offering => 
                offering.Name == name && 
                offering.EndDate >= startOfYear &&
                offering.EndDate <= endOfYear, 
                cancellationToken);
    }
        

    public async Task<List<Timetable>> GetTimetableByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<PeriodId> periodIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Sessions)
            .Where(session => !session.IsDeleted)
            .Select(session => session.PeriodId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Period>()
            .Where(period => periodIds.Contains(period.Id))
            .Select(period => period.Timetable)
            .ToListAsync(cancellationToken);
    }
}