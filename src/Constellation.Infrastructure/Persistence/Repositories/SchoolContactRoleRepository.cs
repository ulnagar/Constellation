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
    public class SchoolContactRoleRepository : ISchoolContactRoleRepository
    {
        private readonly AppDbContext _context;

        public SchoolContactRoleRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<SchoolContactRole> Collection()
        {
            return _context.SchoolContactRoles
                .Include(a => a.SchoolContact)
                .Include(a => a.School);
        }

        public async Task<SchoolContactRole> WithDetails(int id)
        {
            return await Collection()
                .SingleOrDefaultAsync(d => d.Id == id);
        }

        public SchoolContactRole WithFilter(Expression<Func<SchoolContactRole, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<SchoolContactRole> All()
        {
            return Collection()
                .Where(a => !a.IsDeleted)
                .ToList();
        }

        public async Task<ICollection<SchoolContactRole>> AllCurrent()
        {
            return await _context.SchoolContactRoles
                .Include(role => role.SchoolContact)
                .Include(role => role.School)
                .Where(role => role.School.Students.Any(student => !student.IsDeleted) && !role.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContactRole>> AllCurrentFromGrade(Grade grade)
        {
            return await _context.SchoolContactRoles
                .Include(role => role.SchoolContact)
                .Include(role => role.School)
                .Where(role => role.School.Students.Any(student => !student.IsDeleted && student.CurrentGrade == grade) && !role.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContactRole>> AllCurrentWithRole(string selectedRole)
        {
            return await _context.SchoolContactRoles
                .Include(role => role.SchoolContact)
                .Include(role => role.School)
                .Where(role => role.School.Students.Any(student => !student.IsDeleted) && role.Role == selectedRole && !role.IsDeleted)
                .ToListAsync();
        }

        public ICollection<SchoolContactRole> AllWithFilter(Expression<Func<SchoolContactRole, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<SchoolContactRole> AllFromSchool(string schoolCode)
        {
            return Collection()
                .Where(s => s.SchoolCode == schoolCode)
                .Where(a => a.IsDeleted == false)
                .OrderBy(s => s.SchoolContact.LastName)
                .ToList();
        }

        public ICollection<SchoolContactRole> AllWithRole(string role)
        {
            return Collection()
                .Where(s => s.Role == role)
                .Where(a => a.IsDeleted == false)
                .OrderBy(s => s.SchoolContact.LastName)
                .ToList();
        }

        public ICollection<SchoolContactRole> AllFromSchoolWithRole(string schoolCode, string role)
        {
            return Collection()
                .Where(s => s.SchoolCode == schoolCode && s.Role == role)
                .Where(a => a.IsDeleted == false)
                .OrderBy(s => s.SchoolContact.LastName)
                .ToList();
        }

        public ICollection<string> AllRoles()
        {
            return _context.SchoolContactRoles
                .Select(s => s.Role)
                .Distinct()
                .ToList();
        }

        public async Task<ICollection<SchoolContactRole>> FromCoordinatorForLessonsPortal(int contactId)
        {
            return await _context.SchoolContactRoles
                .Include(role => role.School)
                .Where(role => role.SchoolContactId == contactId && !role.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContactRole>> FromCoordinatorForAbsencesPortal(int contactId)
        {
            return await _context.SchoolContactRoles
                .Include(role => role.School)
                .Where(role => role.SchoolContactId == contactId && role.Role == SchoolContactRole.Coordinator && !role.IsDeleted)
                .ToListAsync();
        }

        public async Task<ICollection<string>> ListOfRolesForSelectionAsync()
        {
            return await _context.SchoolContactRoles
                .Select(role => role.Role)
                .Distinct()
                .ToListAsync();
        }

        public async Task<bool> AnyWithId(int id)
        {
            return await _context.SchoolContactRoles
                .AnyAsync(role => role.Id == id);
        }

        public async Task<SchoolContactRole> ForEdit(int id)
        {
            return await _context.SchoolContactRoles
                .SingleOrDefaultAsync(role => role.Id == id);
        }

        public async Task<ICollection<string>> EmailsFromSchoolWithRole(string schoolCode, string role)
        {
            return await _context.SchoolContactRoles
                .Include(record => record.SchoolContact)
                .Include(record => record.School)
                .Where(record => record.SchoolCode == schoolCode && !record.IsDeleted && record.Role == role)
                .Select(record => record.SchoolContact.EmailAddress)
                .ToListAsync();
        }

        public async Task<ICollection<SchoolContactRole>> FromSchoolForLessonsNotifications(string schoolCode)
        {
            return await _context.SchoolContactRoles
                .Include(role => role.SchoolContact)
                .Include(role => role.School)
                .Where(role => !role.IsDeleted && role.SchoolCode == schoolCode)
                .ToListAsync();
        }
    }
}
