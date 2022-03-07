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
    public class SchoolContactRepository : ISchoolContactRepository
    {
        private readonly AppDbContext _context;

        public SchoolContactRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<SchoolContact> Collection()
        {
            return _context.SchoolContacts
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.School);
        }

        public SchoolContact WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(d => d.Id == id);
        }

        public SchoolContact WithFilter(Expression<Func<SchoolContact, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public SchoolContact GetForExistCheck(int id)
        {
            return _context.SchoolContacts
                .SingleOrDefault(contact => contact.Id == id);
        }

        public ICollection<SchoolContact> All()
        {
            return Collection()
                .ToList();
        }


        public ICollection<SchoolContact> AllWithFilter(Expression<Func<SchoolContact, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<SchoolContact> AllFromSchool(string schoolCode)
        {
            return Collection()
                .Where(s => s.Assignments.Any(a => a.School.Code == schoolCode && !a.IsDeleted))
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<SchoolContact> AllWithRole(string role)
        {
            return Collection()
                .Where(s => s.Assignments.Any(a => a.Role == role && !a.IsDeleted))
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<SchoolContact> AllFromSchoolWithRole(string schoolCode, string role)
        {
            return Collection()
                .Where(s => s.Assignments.Any(a => a.School.Code == schoolCode && a.Role == role && !a.IsDeleted))
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public ICollection<SchoolContact> AllWithoutRole()
        {
            return Collection()
                .Where(a => !a.Assignments.Any())
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.LastName)
                .ToList();
        }

        public async Task<SchoolContact> FromEmailForExistCheck(string email)
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .ThenInclude(role => role.School)
                .FirstOrDefaultAsync(contact => contact.EmailAddress == email);
        }

        public async Task<bool> AnyWithId(int id)
        {
            return await _context.SchoolContacts
                .AnyAsync(contact => contact.Id == id);
        }

        public async Task<SchoolContact> FromIdForExistCheck(int id)
        {
            return await _context.SchoolContacts
                .FirstOrDefaultAsync(contact => contact.Id == id);
        }

        public async Task<SchoolContact> ForAudit(int id)
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                    .ThenInclude(role => role.School)
                .FirstOrDefaultAsync(contact => contact.Id == id);
        }

        public async Task<ICollection<SchoolContact>> ScienceTeachersForLessonsPortalAdmin()
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                    .ThenInclude(assignment => assignment.School)
                .Where(contact => !contact.IsDeleted && contact.Assignments.Any(role => !role.IsDeleted && role.Role == SchoolContactRole.SciencePrac))
                .ToListAsync();
        }

        public async Task<bool> IsContactAtSecondarySchool(int id)
        {
            var contact = await _context.SchoolContacts
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.School)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.School.Students)
                .SingleAsync(c => c.Id == id);

            var secondary = contact.Assignments
                .Any(a => a.School.Students.Any(s => s.CurrentGrade >= Core.Enums.Grade.Y07));

            return secondary;
        }

        public async Task<bool> IsContactAtPrimarySchool(int id)
        {
            var contact = await _context.SchoolContacts
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.School)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.School.Students)
                .SingleAsync(c => c.Id == id);

            var primary = contact.Assignments
                .Any(a => a.School.Students.Any(s => s.CurrentGrade <= Core.Enums.Grade.Y06));

            return primary;
        }

        public async Task<ICollection<SchoolContact>> ForSelectionAsync()
        {
            return await _context.SchoolContacts
                .Where(contact => !contact.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContact>> AllWithActiveRoleAsync()
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .ThenInclude(role => role.School)
                .Where(contact => !contact.IsDeleted && contact.Assignments.Any(role => !role.IsDeleted && role.School.Students.Any(student => !student.IsDeleted)))
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContact>> AllWithoutActiveRoleAsync()
        {
            return await _context.SchoolContacts
                .Where(contact => !contact.IsDeleted && (contact.Assignments.All(role => role.IsDeleted) || contact.Assignments.Count == 0 || contact.Assignments.All(role => role.School.Students.All(student => student.IsDeleted))))
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContact>> AllWithStudentsInGradeAsync(Grade grade)
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .ThenInclude(role => role.School)
                .Where(contact => !contact.IsDeleted && contact.Assignments.Any(role => !role.IsDeleted && role.School.Students.Any(student => student.CurrentGrade == grade && !student.IsDeleted)))
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContact>> AllWithRoleAsync(string role)
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .ThenInclude(role => role.School)
                .Where(contact => !contact.IsDeleted && contact.Assignments.Any(assignment => !assignment.IsDeleted && assignment.Role == role && assignment.School.Students.Any(student => !student.IsDeleted)))
                .ToListAsync();
        }

        public async Task<SchoolContact> ForEditAsync(int id)
        {
            return await _context.SchoolContacts
                .SingleOrDefaultAsync(contact => contact.Id == id);
        }

        public async Task<SchoolContact> ForEditFromEmail(string email)
        {
            return await _context.SchoolContacts
                .SingleOrDefaultAsync(contact => contact.EmailAddress == email);
        }

        public async Task<ICollection<string>> EmailAddressesOfAllInRoleAtSchool(string schoolCode, string role)
        {
            return await _context.SchoolContactRoles
                .Where(assignment => assignment.SchoolCode == schoolCode && assignment.Role == role && !assignment.IsDeleted)
                .Select(assignment => assignment.SchoolContact.EmailAddress)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContact>> ForBulkUpdate()
        {
            return await _context.SchoolContacts
                .Include(contact => contact.Assignments)
                .ThenInclude(assignment => assignment.School)
                .ToListAsync();
        }
    }
}
