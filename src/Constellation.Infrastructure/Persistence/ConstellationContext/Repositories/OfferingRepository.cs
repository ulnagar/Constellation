namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;

public class OfferingRepository : IOfferingRepository
{
    private readonly AppDbContext _context;

    public OfferingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Offering?> GetById(
        OfferingId offeringId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Include(offering => offering.Enrolments)
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
            .OrderBy(offering => offering.Course.Grade)
            .ThenBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveForTeacher(
        string StaffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering =>
                offering.Sessions.Any(session =>
                    session.StaffId == StaffId &&
                    !session.IsDeleted &&
                    session.Period.Type != "Other") &&
                offering.StartDate <= DateOnly.FromDateTime(DateTime.Today) &&
                offering.EndDate >= DateOnly.FromDateTime(DateTime.Today))
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .OrderBy(offering => offering.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetByCourseId(
        int courseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Include(offering => offering.Sessions)
            .Where(offering => offering.CourseId == courseId)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByGrade(
        Grade grade, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .Where(offering => 
                offering.Course.Grade == grade &&
                offering.IsCurrent)
            .ToListAsync(cancellationToken);

    public async Task<List<Offering>> GetActiveByCourseId(
        int courseId,
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
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                // enrolment was created before the absence date
                enrolment.DateCreated < AbsenceDate &&
                // enrolment is either still current (not deleted) OR was deleted after the absence date
                (!enrolment.IsDeleted || enrolment.DateDeleted.Value.Date > AbsenceDate) &&
                // offering ends after the absence date
                enrolment.Offering.EndDate > DateOnly.FromDateTime(AbsenceDate) &&
                enrolment.Offering.Sessions.Any(session =>
                    // session was created before the absence date
                    session.DateCreated < AbsenceDate &&
                    // session is either still current (not deleted) OR was deleted after the absence date
                    (!session.IsDeleted || session.DateDeleted.Value.Date > AbsenceDate) &&
                    // session is for the same day as the absence
                    session.Period.Day == DayNumber))
            .Select(enrolment => enrolment.Offering)
            .Distinct()
            .ToListAsync(cancellationToken);

    // Method is not async as we are passing the task to another method
    public Task<List<Offering>> GetCurrentEnrolmentsFromStudentForDate(
        string studentId,
        DateOnly AbsenceDate,
        int DayNumber,
        CancellationToken cancellationToken = default) =>
        GetCurrentEnrolmentsFromStudentForDate(studentId, AbsenceDate.ToDateTime(TimeOnly.MinValue), DayNumber, cancellationToken);

    public async Task<List<Offering>> GetByStudentId(
        string studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                !enrolment.IsDeleted)
        .Select(enrolment => enrolment.Offering)
        .Distinct()
        .ToListAsync(cancellationToken);

    public async Task<Offering> GetFromYearAndName(
        int year, 
        string name, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Offering>()
            .SingleOrDefaultAsync(offering => offering.Name == name && offering.EndDate.Year == year, cancellationToken);
}