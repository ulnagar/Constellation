using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using static Constellation.Core.Errors.DomainErrors.ClassCovers;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class TimetablePeriodRepository : ITimetablePeriodRepository
    {
        private readonly AppDbContext _context;

        public TimetablePeriodRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TimetablePeriod>> GetForOfferingOnDay(
            int offeringId,
            DateTime absenceDate,
            int dayNumber,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<OfferingSession>()
                .Where(session => session.OfferingId == offeringId &&
                    // session was created before the absence date
                    session.DateCreated < absenceDate &&
                    // session is still current (not deleted) OR session was deleted after absence date
                    (!session.IsDeleted || session.DateDeleted.Value.Date > absenceDate) &&
                    // session is active on the absence day
                    session.Period.Day == dayNumber)
                .Select(session => session.Period)
                .Distinct()
                .ToListAsync(cancellationToken);

        // Method is not async as we are passing the async task to another method
        public Task<List<TimetablePeriod>> GetForOfferingOnDay(
            int offeringId,
            DateOnly absenceDate,
            int dayNumber,
            CancellationToken cancellationToken = default) =>
            GetForOfferingOnDay(offeringId, absenceDate.ToDateTime(TimeOnly.MinValue), dayNumber, cancellationToken);

        private IQueryable<TimetablePeriod> Collection()
        {
            return _context.Periods
                .Include(p => p.OfferingSessions)
                    .ThenInclude(offering => offering.Offering)
                .Include(p => p.OfferingSessions)
                    .ThenInclude(offering => offering.Room)
                .Include(p => p.OfferingSessions)
                    .ThenInclude(offering => offering.Teacher);
        }

        public async Task<List<TimetablePeriod>> GetByDayAndOfferingId(
            int dayNumber,
            int offeringId,
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

        public TimetablePeriod WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public TimetablePeriod WithFilter(Expression<Func<TimetablePeriod, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<TimetablePeriod> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<TimetablePeriod> AllWithFilter(Expression<Func<TimetablePeriod, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<TimetablePeriod> AllFromDay(int day)
        {
            return Collection()
                .Where(p => p.IsDeleted == false)
                .Where(p => p.Day == day)
                .ToList();
        }

        public ICollection<TimetablePeriod> AllActive()
        {
            return Collection()
                .Where(p => p.IsDeleted == false)
                .ToList();
        }

        public ICollection<TimetablePeriod> AllForStudent(string studentId)
        {
            var enrolments = _context.Enrolments.Where(e => !e.IsDeleted && e.StudentId == studentId);
            var sessions = enrolments.SelectMany(e => e.Offering.Sessions);
            var periods = sessions.Select(s => s.Period);

            return periods.Distinct().ToList();
        }

        public async Task<ICollection<TimetablePeriod>> ForSelectionAsync()
        {
            return await _context.Periods
                .Where(period => !period.IsDeleted)
                .OrderBy(period => period.Timetable)
                .ThenBy(period => period.Day)
                .ThenBy(period => period.StartTime)
                .ToListAsync();
        }

        public async Task<ICollection<TimetablePeriod>> ForGraphicalDisplayAsync()
        {
            return await _context.Periods
                .Where(period => !period.IsDeleted)
                .ToListAsync();
        }

        public async Task<TimetablePeriod> ForEditAsync(int id)
        {
            return await _context.Periods
                .SingleOrDefaultAsync(period => period.Id == id);
        }
    }
}
