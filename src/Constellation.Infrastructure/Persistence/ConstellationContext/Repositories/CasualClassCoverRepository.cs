using Constellation.Application.Extensions;
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
    public class CasualClassCoverRepository : IClassCoverRepository<CasualClassCover>
    {
        private readonly AppDbContext _context;

        public CasualClassCoverRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<CasualClassCover> Collection()
        {
            return _context.Set<CasualClassCover>()
                .Include(cc => cc.Offering)
                    .ThenInclude(offering => offering.Course)
                        .ThenInclude(course => course.Faculty)
                            .ThenInclude(faculty => faculty.Members)
                                .ThenInclude(member => member.Staff)
                .Include(cc => cc.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Room)
                .Include(cc => cc.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Teacher)
                .Include(cc => cc.Casual)
                    .ThenInclude(casual => casual.School)
                .Include(cc => cc.AdobeConnectOperations)
                    .ThenInclude(operation => operation.Room)
                .Include(cc => cc.MSTeamOperations as ICollection<CasualMSTeamOperation>)
                    .ThenInclude(operation => operation.Offering);
        }

        public CasualClassCover WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(c => c.Id == id);
        }

        public CasualClassCover WithFilter(Expression<Func<CasualClassCover, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public CasualClassCover GetForExistCheck(int id)
        {
            return _context.Set<CasualClassCover>()
                .SingleOrDefault(cover => cover.Id == id);
        }

        public ICollection<CasualClassCover> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<CasualClassCover> AllWithFilter(Expression<Func<CasualClassCover, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<CasualClassCover> AllUpcoming()
        {
            return Collection()
                .Where(c => c.IsDeleted == false)
                .ToList()
                .Where(c => c.IsCurrent() || c.IsFuture())
                .ToList();
        }

        public ICollection<CasualClassCover> AllOutdated()
        {
            var yesterday = DateTime.Today.AddDays(-1);

            return Collection()
                .Where(cc => cc.EndDate < yesterday || cc.IsDeleted)
                .ToList();
        }

        public async Task<CasualClassCover> ForEditAsync(int id)
        {
            return await _context.Covers
                .Include(cover => cover.Offering)
                .ThenInclude(offering => offering.Course)
                .ThenInclude(course => course.Faculty)
                .ThenInclude(faculty => faculty.Members)
                .ThenInclude(member => member.Staff)
                .Include(cover => cover.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .Include(cover => (cover as CasualClassCover).AdobeConnectOperations)
                .Include(cover => (cover as CasualClassCover).MSTeamOperations)
                .Include(cover => (cover as CasualClassCover).Casual)
                .SingleOrDefaultAsync(cover => cover.Id == id) as CasualClassCover;
        }

        public async Task<ICollection<CasualClassCover>> ForClassworkNotifications(DateTime absenceDate, int offeringId)
        {
            return await _context.Covers
                .OfType<CasualClassCover>()
                .Include(cover => cover.Casual)
                .Where(cover => !cover.IsDeleted && cover.StartDate <= absenceDate && cover.EndDate >= absenceDate && cover.OfferingId == offeringId)
                .ToListAsync();
        }
    }
}