using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AppDbContext _context;

        public StaffRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Staff> Collection()
        {
            return _context.Staff
                .Include(s => s.AdobeConnectOperations)
                    .ThenInclude(operation => operation.Room)
                .Include(s => s.AdobeConnectGroupOperations)
                .Include(s => s.School)
                .Include(s => s.CourseSessions)
                    .ThenInclude(session => session.Offering)
                        .ThenInclude(offering => offering.Course)
                .Include(s => s.CourseSessions)
                    .ThenInclude(session => session.Period)
                .Include(s => s.CourseSessions)
                    .ThenInclude(session => session.Room);
        }

        public Staff WithDetails(string id)
        {
            return Collection()
                .SingleOrDefault(d => d.StaffId == id);
        }

        public Staff WithFilter(Expression<Func<Staff, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public async Task<Staff> GetForExistCheck(string id)
        {
            return await _context.Staff
                .SingleOrDefaultAsync(staff => staff.StaffId == id);
        }

        public async Task<Staff> WithFilterAsync(Expression<Func<Staff, bool>> predicate)
        {
            return await Collection()
                .FirstOrDefaultAsync(predicate);
        }

        public ICollection<Staff> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<Staff> AllWithFilter(Expression<Func<Staff, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public async Task<ICollection<Staff>> AllActiveAsync()
        {
            return await _context.Staff
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToListAsync();
        }

        public ICollection<Staff> AllActive()
        {
            return Collection()
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<Staff> AllInactive()
        {
            return Collection()
                .Where(s => s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<Staff> AllFromSchool(string code)
        {
            return Collection()
                .Where(s => s.SchoolCode == code)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<Staff> AllActiveFromSchool(string code)
        {
            return Collection()
                .Where(s => !s.IsDeleted && s.SchoolCode == code)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<Staff> AllFromFaculty(Faculty faculty)
        {
            return Collection()
                .Where(s => s.Faculty.HasFlag(faculty) && !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<Staff> AllWithActiveClasses()
        {
            return Collection()
                .Where(s => s.IsDeleted == false)
                .Where(
                    s =>
                        s.CourseSessions.Any(
                            d => d.Offering.StartDate < DateTime.Now && d.Offering.EndDate > DateTime.Now))
                .ToList();
        }

        public ICollection<Staff> AllWithoutAdobeConnectInfo()
        {
            return Collection()
                .Where(s => s.AdobeConnectPrincipalId == null || s.AdobeConnectPrincipalId.Trim() == string.Empty)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public async Task<Staff> FromEmailForExistCheck(string email)
        {
            return await _context.Staff
                .FirstOrDefaultAsync(member => email.Contains(member.EmailAddress));
        }

        public async Task<Staff> GetFromName(string name)
        {
            var expandedName = name.ToLowerInvariant().Trim().Split(' ');
            var fixedName = "";

            foreach (var item in expandedName)
            {
                fixedName += item;
                if (expandedName.Last() != item)
                    fixedName += ".";
            }

            return await _context.Staff
                .SingleOrDefaultAsync(member => member.PortalUsername.Contains(fixedName));
        }

        public async Task<Staff> FromIdForExistCheck(string id)
        {
            return await _context.Staff
                .FirstOrDefaultAsync(member => member.StaffId == id);
        }

        public async Task<ICollection<Staff>> ForListAsync(Expression<Func<Staff, bool>> predicate)
        {
            return await _context.Staff
                .Include(staff => staff.School)
                .OrderBy(staff => staff.LastName)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Staff> ForDetailDisplayAsync(string id)
        {
            return await _context.Staff
                .Include(staff => staff.School)
                .ThenInclude(school => school.StaffAssignments)
                .ThenInclude(role => role.SchoolContact)
                .Include(staff => staff.CourseSessions)
                .ThenInclude(session => session.Offering)
                .ThenInclude(offering => offering.Course)
                .Include(staff => staff.CourseSessions)
                .ThenInclude(session => session.Period)
                .Include(staff => staff.CourseSessions)
                .ThenInclude(session => session.Room)
                .SingleOrDefaultAsync(staff => staff.StaffId == id);
        }

        public async Task<ICollection<Staff>> ForSelectionAsync()
        {
            return await _context.Staff
                .Where(staff => !staff.IsDeleted)
                .ToListAsync();
        }

        public async Task<Staff> ForEditAsync(string id)
        {
            return await _context.Staff
                .SingleOrDefaultAsync(staff => staff.StaffId == id);
        }

        public async Task<bool> AnyWithId(string id)
        {
            return await _context.Staff
                .AnyAsync(staff => staff.StaffId == id);
        }

        public async Task<Staff> ForDeletion(string id)
        {
            return await _context.Staff
                .Include(staff => staff.CourseSessions)
                .Include(staff => staff.AdobeConnectGroupOperations)
                .SingleOrDefaultAsync(staff => staff.StaffId == id);
        }
    }
}
