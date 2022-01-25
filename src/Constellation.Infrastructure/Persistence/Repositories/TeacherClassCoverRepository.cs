using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class TeacherClassCoverRepository : IClassCoverRepository<TeacherClassCover>
    {
        private readonly AppDbContext _context;

        public TeacherClassCoverRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<TeacherClassCover> Collection()
        {
            return _context.Set<TeacherClassCover>()
                .Include(cc => cc.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Room)
                .Include(cc => cc.Offering)
                    .ThenInclude(offering => offering.Sessions)
                        .ThenInclude(session => session.Teacher)
                .Include(cc => cc.Staff)
                    .ThenInclude(staff => staff.School)
                .Include(cc => cc.AdobeConnectOperations)
                    .ThenInclude(operation => operation.Room)
                .Include(cc => cc.MSTeamOperations as ICollection<TeacherMSTeamOperation>)
                    .ThenInclude(operation => operation.Offering);
        }

        public TeacherClassCover WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(c => c.Id == id);
        }

        public TeacherClassCover WithFilter(Expression<Func<TeacherClassCover, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public TeacherClassCover GetForExistCheck(int id)
        {
            return _context.Set<TeacherClassCover>()
                .SingleOrDefault(cover => cover.Id == id);
        }

        public ICollection<TeacherClassCover> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<TeacherClassCover> AllWithFilter(Expression<Func<TeacherClassCover, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<TeacherClassCover> AllUpcoming()
        {
            return Collection()
                .Where(c => c.IsDeleted == false)
                .ToList()
                .Where(c => c.IsCurrent() || c.IsFuture())
                .ToList();
        }

        public ICollection<TeacherClassCover> AllOutdated()
        {
            var yesterday = DateTime.Today.AddDays(-1);

            return Collection()
                .Where(cc => cc.EndDate < yesterday || cc.IsDeleted)
                .ToList();
        }

        public async Task<TeacherClassCover> ForEditAsync(int id)
        {
            return await _context.Covers
                .Include(cover => cover.Offering)
                .ThenInclude(offering => offering.Course)
                .ThenInclude(course => course.HeadTeacher)
                .Include(cover => cover.Offering)
                .ThenInclude(offering => offering.Sessions)
                .ThenInclude(session => session.Teacher)
                .SingleOrDefaultAsync(cover => cover.Id == id) as TeacherClassCover;
        }

        public async Task<ICollection<TeacherClassCover>> ForClassworkNotifications(DateTime absenceDate, int offeringId)
        {
            return await _context.Covers
                .OfType<TeacherClassCover>()
                .Include(cover => cover.Staff)
                .Where(cover => !cover.IsDeleted && cover.StartDate <= absenceDate && cover.EndDate >= absenceDate && cover.OfferingId == offeringId)
                .ToListAsync();
        }
    }
}