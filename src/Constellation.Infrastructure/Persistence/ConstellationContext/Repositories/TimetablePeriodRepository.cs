using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class TimetablePeriodRepository : ITimetablePeriodRepository
    {
        private readonly AppDbContext _context;

        public TimetablePeriodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TimetablePeriod> GetById(
            int id,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<TimetablePeriod>()
                .SingleOrDefaultAsync(period => period.Id == id, cancellationToken);

        public async Task<List<TimetablePeriod>> GetForOfferingOnDay(
            OfferingId offeringId,
            DateTime absenceDate,
            int dayNumber,
            CancellationToken cancellationToken = default)
        {
            List<Session> sessions = await _context
                .Set<Session>()
                .Where(session => session.OfferingId == offeringId &&
                    // session was created before the absence date
                    session.CreatedAt < absenceDate &&
                    // session is still current (not deleted) OR session was deleted after absence date
                    (!session.IsDeleted || session.DeletedAt.Date > absenceDate))
                .ToListAsync(cancellationToken);

            return await _context
                .Set<TimetablePeriod>()
                .Where(period =>
                    period.Day == dayNumber &&
                    sessions.Select(session => session.PeriodId).Contains(period.Id))
                .ToListAsync(cancellationToken);
        }

        // Method is not async as we are passing the async task to another method
        public Task<List<TimetablePeriod>> GetForOfferingOnDay(
            OfferingId offeringId,
            DateOnly absenceDate,
            int dayNumber,
            CancellationToken cancellationToken = default) =>
            GetForOfferingOnDay(offeringId, absenceDate.ToDateTime(TimeOnly.MinValue), dayNumber, cancellationToken);

        public async Task<List<TimetablePeriod>> GetCurrent(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<TimetablePeriod>()
                .Where(period => !period.IsDeleted)
                .ToListAsync(cancellationToken);

        public async Task<double> TotalDurationForCollectionOfPeriods(
            List<int> PeriodIds,
            CancellationToken cancellationToken = default)
        {
            List<TimetablePeriod> periods = await _context
                .Set<TimetablePeriod>()
                .Where(period => PeriodIds.Contains(period.Id))
                .ToListAsync(cancellationToken);
    
            return periods
                .Sum(period => period.EndTime.Subtract(period.StartTime).TotalMinutes);
        }


        public async Task<List<TimetablePeriod>> GetByDayAndOfferingId(
            int dayNumber,
            OfferingId offeringId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<TimetablePeriod>()
                .Where(period => 
                    period.Day == dayNumber && 
                    period.OfferingSessions.Any(session => 
                        !session.IsDeleted && 
                        session.OfferingId == offeringId))
                .ToListAsync(cancellationToken);

        public async Task<List<TimetablePeriod>> GetAll(CancellationToken cancellationToken = default) =>
            await _context
                .Set<TimetablePeriod>()
                .Where(period => !period.IsDeleted)
                .ToListAsync(cancellationToken);

        public async Task<List<TimetablePeriod>> GetAllFromTimetable(
            List<string> timetables, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<TimetablePeriod>()
                .Where(period => !period.IsDeleted && timetables.Contains(period.Timetable))
                .ToListAsync(cancellationToken);

        public ICollection<TimetablePeriod> AllFromDay(int day)
        {
            return _context.Set<TimetablePeriod>()
                .Where(p => p.IsDeleted == false)
                .Where(p => p.Day == day)
                .ToList();
        }

        public async Task<ICollection<TimetablePeriod>> ForSelectionAsync()
        {
            return await _context.Set<TimetablePeriod>()
                .Where(period => !period.IsDeleted)
                .OrderBy(period => period.Timetable)
                .ThenBy(period => period.Day)
                .ThenBy(period => period.StartTime)
                .ToListAsync();
        }

        public async Task<ICollection<TimetablePeriod>> ForGraphicalDisplayAsync()
        {
            return await _context.Set<TimetablePeriod>()
                .Where(period => !period.IsDeleted)
                .ToListAsync();
        }

        public async Task<TimetablePeriod> ForEditAsync(int id)
        {
            return await _context.Set<TimetablePeriod>()
                .SingleOrDefaultAsync(period => period.Id == id);
        }
    }
}
