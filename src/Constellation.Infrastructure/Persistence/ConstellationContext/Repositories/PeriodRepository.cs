namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Timetables.Enums;
using Constellation.Core.Models.Timetables.ValueObjects;
using Constellation.Core.Models.Tutorials.Identifiers;
using Core.Models.Offerings;
using Core.Models.Offerings.Identifiers;
using Core.Models.Timetables;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.Repositories;
using Core.Models.Tutorials;
using Microsoft.EntityFrameworkCore;

public class PeriodRepository : IPeriodRepository
{
    private readonly AppDbContext _context;

    public PeriodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Period> GetById(
        PeriodId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .SingleOrDefaultAsync(period => period.Id == id, cancellationToken);

    public async Task<Period> GetByPeriodCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        char timetablePrefix = code.Length > 1 ? code.First() : '\0';
        char periodCode = code.Last();

        Timetable timetable = Timetable.FromPrefix(timetablePrefix);

        return await _context
            .Set<Period>()
            .Where(period =>
                period.Timetable == timetable &&
                period.PeriodCode == periodCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Period>> GetForTutorialOnDay(
        TutorialId tutorialId,
        DateOnly absenceDate,
        PeriodWeek week,
        PeriodDay day,
        CancellationToken cancellationToken = default)
    {
        List<TutorialSession> sessions = await _context
            .Set<TutorialSession>()
            .Where(session =>
                session.TutorialId == tutorialId &&
                // session was created before the absence date
                session.CreatedAt < absenceDate.ToDateTime(TimeOnly.MinValue) &&
                // session is still current (not deleted) OR session was deleted after absence date
                (!session.IsDeleted || session.DeletedAt.Date > absenceDate.ToDateTime(TimeOnly.MinValue)))
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Period>()
            .Where(period =>
                period.Week == week &&
                period.Day == day &&
                sessions.Select(session => session.PeriodId).Contains(period.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Period>> GetForOfferingOnDay(    
        OfferingId offeringId,
        DateTime absenceDate,
        PeriodWeek week, 
        PeriodDay day,
        CancellationToken cancellationToken = default)
    {
        List<Session> sessions = await _context
            .Set<Session>()
            .Where(session => 
                session.OfferingId == offeringId &&
                // session was created before the absence date
                session.CreatedAt < absenceDate &&
                // session is still current (not deleted) OR session was deleted after absence date
                (!session.IsDeleted || session.DeletedAt.Date > absenceDate))
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Period>()
            .Where(period =>
                period.Week == week &&
                period.Day == day &&
                sessions.Select(session => session.PeriodId).Contains(period.Id))
            .ToListAsync(cancellationToken);
    }

    // Method is not async as we are passing the async task to another method
    public Task<List<Period>> GetForOfferingOnDay(
        OfferingId offeringId,
        DateOnly absenceDate,
        PeriodWeek week, 
        PeriodDay day,
        CancellationToken cancellationToken = default) =>
        GetForOfferingOnDay(offeringId, absenceDate.ToDateTime(TimeOnly.MinValue), week, day, cancellationToken);

    public async Task<List<Period>> GetCurrent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .Where(period => !period.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<double> TotalDurationForCollectionOfPeriods(
        List<PeriodId> periodIds,
        CancellationToken cancellationToken = default)
    {
        List<Period> periods = await _context
            .Set<Period>()
            .Where(period => periodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);
    
        return periods
            .Sum(period => period.Duration);
    }

    public async Task<List<Period>> GetByDayAndOfferingId(
        int day,
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        PeriodWeek targetWeek = PeriodWeek.FromDayNumber(day);
        PeriodDay targetDay = PeriodDay.FromDayNumber(day);

        List<Session> sessions = await _context
            .Set<Offering>()
            .SelectMany(offering => offering.Sessions)
            .Where(session =>
                !session.IsDeleted &&
                session.OfferingId == offeringId)
            .ToListAsync(cancellationToken);

        List<PeriodId> sessionPeriodIds = sessions
            .Select(session => session.PeriodId)
            .ToList();

        return await _context
            .Set<Period>()
            .Where(period =>
                period.Week == targetWeek &&
                period.Day == targetDay &&
                sessionPeriodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Period>> GetByWeekAndDay(
        PeriodWeek week,
        PeriodDay day,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .Where(period =>
                period.Week == week &&
                period.Day == day)
            .ToListAsync(cancellationToken);

    public async Task<List<Period>> GetAll(CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .Where(period => !period.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Period>> GetAllFromTimetable(
        List<Timetable> timetables, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .Where(period => !period.IsDeleted && timetables.Contains(period.Timetable))
            .ToListAsync(cancellationToken);

    public async Task<List<Period>> GetListFromIds(
        List<PeriodId> periodIds, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Period>()
            .Where(period => periodIds.Contains(period.Id))
            .ToListAsync(cancellationToken);

    public void Insert(Period period) => _context.Set<Period>().Add(period);
}