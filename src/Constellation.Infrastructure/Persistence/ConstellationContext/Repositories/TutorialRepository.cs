namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Enums;
using Constellation.Core.Models.Timetables.Identifiers;
using Constellation.Core.Models.Tutorials.ValueObjects;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Enums;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class TutorialRepository : ITutorialRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public TutorialRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Tutorial> GetById(
        TutorialId tutorialId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .FirstOrDefaultAsync(tutorial => tutorial.Id == tutorialId,
                cancellationToken);

    public async Task<List<Tutorial>> GetWithLinkedTeam(
        Guid teamId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session => !session.IsDeleted) &&
                tutorial.Teams.Any(team => team.TeamId == teamId))
            .ToListAsync(cancellationToken);

    public async Task<Tutorial> GetByNameAndYear(
        int year,
        TutorialName name,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate.Year == year &&
                ((string)tutorial.Name).Contains(name.Value))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Tutorial>> GetAll(
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = _dateTime.FirstDayOfYear;

        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate >= startOfYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                //tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session => !session.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<Tutorial>> GetInactive(
        CancellationToken cancellationToken = default)
    {
        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate >= _dateTime.FirstDayOfYear &&
                 (tutorial.Sessions.Count == 0 ||
                 tutorial.Sessions.All(session => session.IsDeleted)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetActiveForTeacher(
        StaffId staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session =>
                    !session.IsDeleted &&
                    session.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<List<Tutorial>> GetActiveForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TutorialId> tutorialIds = _context
            .Set<Enrolment>()
            .OfType<TutorialEnrolment>()
            .Where(enrolment => enrolment.StudentId == studentId)
            .Select(enrolment => enrolment.TutorialId);

        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.StartDate <= _dateTime.Today &&
                tutorial.EndDate >= _dateTime.Today &&
                tutorial.Sessions.Any(session => !session.IsDeleted) &&
                tutorialIds.Contains(tutorial.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetAllForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TutorialId> tutorialIds = _context
            .Set<Enrolment>()
            .OfType<TutorialEnrolment>()
            .Where(enrolment => enrolment.StudentId == studentId)
            .Select(enrolment => enrolment.TutorialId);

        return await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate.Year == _dateTime.CurrentYear &&
                tutorialIds.Contains(tutorial.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tutorial>> GetCurrentEnrolmentsFromStudentForDate(
    StudentId studentId,
    DateOnly absenceDate,
    PeriodWeek week,
    PeriodDay day,
    CancellationToken cancellationToken = default)
    {
        List<PeriodId> periodIds = await _context
            .Set<Period>()
            .Where(period =>
                period.Week == week &&
                period.Day == day)
            .Select(period => period.Id)
            .ToListAsync(cancellationToken);

        List<TutorialId> tutorialIds = await _context
            .Set<Enrolment>()
            .Where(enrolment => enrolment.StudentId == studentId &&
                // enrolment was created before the absence date
                enrolment.CreatedAt < absenceDate.ToDateTime(TimeOnly.MinValue) &&
                // enrolment is either still current (not deleted) OR was deleted after the absence date
                (!enrolment.IsDeleted || enrolment.DeletedAt.Date > absenceDate.ToDateTime(TimeOnly.MinValue)))
            .OfType<TutorialEnrolment>()
            .Select(enrolment => enrolment.TutorialId)
            .ToListAsync(cancellationToken);

        List<Tutorial> tutorials = await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorialIds.Contains(tutorial.Id) &&
                // offering ends after the absence date
                tutorial.EndDate > absenceDate &&
                tutorial.Sessions.Any(session =>
                    // session was created before the absence date
                    session.CreatedAt < absenceDate.ToDateTime(TimeOnly.MinValue) &&
                    // session is either still current (not deleted) OR was deleted after the absence date
                    (!session.IsDeleted || session.DeletedAt.Date > absenceDate.ToDateTime(TimeOnly.MinValue)) &&
                    // session is for the same day as the absence
                    periodIds.Contains(session.PeriodId)))
            .Distinct()
            .ToListAsync(cancellationToken);

        return tutorials;
    }

    public async Task<bool> DoesTutorialAlreadyExist(
        TutorialName name,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Tutorial>()
            .AnyAsync(tutorial =>
                !tutorial.IsDeleted &&
                tutorial.Name == name &&
                ((tutorial.StartDate <= endDate && tutorial.StartDate >= startDate) || // DB Tutorial starts during new tutorial
                (tutorial.EndDate <= endDate && tutorial.EndDate >= startDate) || //DB Tutorial ends during new tutorial
                (tutorial.StartDate <= startDate && tutorial.EndDate >= endDate)), // DB Tutorial starts before and ends after new tutorial
                cancellationToken);

    public async Task<int> GetNextTutorialNameSequence(
        TutorialName name,
        CancellationToken cancellationToken)
    {
        List<TutorialName> existing = await _context
            .Set<Tutorial>()
            .Where(tutorial =>
                tutorial.StartDate > _dateTime.FirstDayOfYear &&
                ((string)tutorial.Name).Contains(name.Value))
            .Select(tutorial => tutorial.Name)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
            return 0;

        string existingSequence = existing.Last().Value.Last().ToString();

        return Convert.ToInt32(existingSequence);
    }

    public void Insert(Tutorial tutorial) =>
        _context.Set<Tutorial>().Add(tutorial);

    public async Task<Request> GetRequestById(
        RequestId requestId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .FirstOrDefaultAsync(request => request.Id == requestId, cancellationToken);

    public async Task<List<Request>> GetAllRequests(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .ToListAsync(cancellationToken);

    public async Task<int> CountPendingRequests(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .Where(request => 
                request.Status == RequestStatus.Requested &&
                request.CreatedAt.Year == _dateTime.CurrentYear)
            .CountAsync(cancellationToken);

    public async Task<int> CountApprovedRequests(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .Where(request => 
                request.Status == RequestStatus.Approved &&
                request.CreatedAt.Year == _dateTime.CurrentYear)
            .CountAsync(cancellationToken);

    public async Task<List<Request>> GetPendingRequests(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .Where(request => 
                request.Status == RequestStatus.Requested &&
                request.CreatedAt.Year == _dateTime.CurrentYear)
            .ToListAsync(cancellationToken);

    public async Task<List<Request>> GetApprovedRequests(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .Where(request => 
                request.Status == RequestStatus.Approved &&
                request.CreatedAt.Year == _dateTime.CurrentYear)
            .ToListAsync(cancellationToken);

    public async Task<List<Request>> GetRequestsForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Request>()
            .Where(request =>
                request.StudentId == studentId &&
                request.CreatedAt.Year == _dateTime.CurrentYear)
            .ToListAsync(cancellationToken);

    public void Insert(Request request) =>
        _context.Set<Request>().Add(request);
}
