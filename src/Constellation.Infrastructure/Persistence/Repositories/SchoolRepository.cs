using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
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
    public class SchoolRepository : ISchoolRepository
    {
        private readonly AppDbContext _context;

        public SchoolRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<School> Collection()
        {
            return _context.Schools
                .Include(s => s.Staff)
                    .ThenInclude(staff => staff.CourseSessions)
                        .ThenInclude(session => session.Offering)
                .Include(s => s.StaffAssignments)
                    .ThenInclude(assignment => assignment.SchoolContact)
                .Include(s => s.Students)
                    .ThenInclude(student => student.Enrolments)
                        .ThenInclude(enrolment => enrolment.Offering);
        }

        public School WithDetails(string id)
        {
            return Collection()
                .SingleOrDefault(d => d.Code == id);
        }

        public School WithFilter(Expression<Func<School, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public ICollection<School> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<School> AllWithFilter(Expression<Func<School, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<School> AllWithStudents()
        {
            return Collection()
                .Where(s => s.Students.Any(a => !a.IsDeleted))
                .OrderBy(s => s.Name)
                .ToList();
        }

        public async Task<ICollection<School>> AllWithStudentsForAbsenceSettingsAsync()
        {
            return await _context.Schools
                .Where(s => s.Students.Any(a => !a.IsDeleted))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public ICollection<School> AllWithStaff()
        {
            return Collection()
                .Where(s => s.Staff.Any(a => !a.IsDeleted))
                .OrderBy(s => s.Name)
                .ToList();
        }

        public ICollection<School> AllWithBoth()
        {
            return Collection()
                .Where(s => s.Students.Any(a => !a.IsDeleted) && s.Staff.Any(a => !a.IsDeleted))
                .OrderBy(s => s.Name)
                .ToList();
        }

        public ICollection<School> AllWithEither()
        {
            return Collection()
                 .Where(s => s.Students.Any(a => !a.IsDeleted) || s.Staff.Any(a => !a.IsDeleted))
                 .OrderBy(s => s.Name).ToList();
        }

        public ICollection<School> AllWithNeither()
        {
            return Collection()
                .Where(s => !s.Students.Any(a => !a.IsDeleted) && !s.Staff.Any(a => !a.IsDeleted))
                .OrderBy(s => s.Name)
                .ToList();
        }

        public async Task<IDictionary<string, string>> AllForLessonsPortal()
        {
            var schools = await _context.Schools
                    .Where(school => school.Students.Any(student => !student.IsDeleted))
                    .OrderBy(school => school.Name)
                    .ToListAsync();

            var dict = new Dictionary<string, string>();
            foreach (var school in schools)
            {
                dict.Add(school.Code, school.Name);
            }

            return dict;
        }

        public async Task<ICollection<School>> ForSelectionAsync()
        {
            return await _context.Schools
                .ToListAsync();
        }

        public async Task<ICollection<School>> ForListAsync(Expression<Func<School, bool>> predicate)
        {
            return await _context.Schools
                .OrderBy(school => school.Name)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<School> ForEditAsync(string id)
        {
            return await _context.Schools
                .SingleOrDefaultAsync(school => school.Code == id);
        }

        public async Task<School> ForDetailDisplayAsync(string id)
        {
            return await _context.Schools
                .Include(school => school.Students)
                .ThenInclude(student => student.Enrolments)
                .ThenInclude(enrolment => enrolment.Offering)
                .Include(school => school.Staff)
                .ThenInclude(staff => staff.CourseSessions)
                .ThenInclude(session => session.Offering)
                .Include(school => school.StaffAssignments)
                .ThenInclude(assignment => assignment.SchoolContact)
                .SingleOrDefaultAsync(school => school.Code == id);
        }

        public async Task<bool> IsPartnerSchoolWithStudents(string code)
        {
            return await _context.Schools
                .AnyAsync(school => school.Code == code && school.Students.Any(student => !student.IsDeleted));
        }

        public async Task<bool> AnyWithId(string id)
        {
            return await _context.Schools
                .AnyAsync(school => school.Code == id);
        }

        public async Task<ICollection<School>> ForBulkUpdate()
        {
            return await _context.Schools
                .Include(school => school.StaffAssignments)
                .ThenInclude(role => role.SchoolContact)
                .ToListAsync();
        }

        public IList<MapLayer> GetForMapping(IList<string> schoolCodes)
        {
            var vm = new List<MapLayer>();

            var schools = _context.Schools
                .Include(school => school.Students)
                .Include(school => school.Staff);

            var filteredSchools = (schoolCodes.Count > 0)
                    ? schools.Where(school => schoolCodes.Any(code => code == school.Code))
                    : schools;

            vm.Add(MapHelpers.MapLayerBuilder(
                filteredSchools.Where(school =>
                    school.Students.Any(student => !student.IsDeleted) &&
                    school.Staff.All(staff => staff.IsDeleted)),
                "Students only",
                "blue"));

            vm.Add(MapHelpers.MapLayerBuilder(
                filteredSchools.Where(school =>
                    school.Students.All(student => student.IsDeleted) &&
                    school.Staff.Any(staff => !staff.IsDeleted)),
                "Staff only",
                "red"));

            vm.Add(MapHelpers.MapLayerBuilder(
                filteredSchools.Where(school =>
                    school.Students.Any(student => !student.IsDeleted) &&
                    school.Staff.Any(staff => !staff.IsDeleted)),
                "Both Students and Staff",
                "green"));

            return vm;
        }
    }
}
