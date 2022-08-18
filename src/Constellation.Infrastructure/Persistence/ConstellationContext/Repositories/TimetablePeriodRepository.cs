using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class TimetablePeriodRepository : ITimetablePeriodRepository
    {
        private readonly AppDbContext _context;

        public TimetablePeriodRepository(AppDbContext context)
        {
            _context = context;
        }

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
