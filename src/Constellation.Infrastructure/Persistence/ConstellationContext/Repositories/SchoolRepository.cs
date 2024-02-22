using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class SchoolRepository : ISchoolRepository
    {
        private readonly AppDbContext _context;

        public SchoolRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<School>> GetAllActive(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<School>()
                .Where(school => 
                    school.Students.Any(student => !student.IsDeleted) ||
                    school.Staff.Any(staff => !staff.IsDeleted))
                .ToListAsync(cancellationToken);


        private IQueryable<School> Collection()
        {
            return _context.Schools
                .Include(s => s.Staff)
                .Include(s => s.Students)
                    .ThenInclude(student => student.Enrolments);
        }

        public async Task<List<School>> GetListFromIds(
            List<string> schoolCodes, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<School>()
                .Where(school => schoolCodes.Contains(school.Code))
                .ToListAsync(cancellationToken);

        public void Insert(School school) =>
            _context.Set<School>().Add(school);

        public async Task<School?> GetById(
            string id,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<School>()
                .FirstOrDefaultAsync(school => school.Code == id, cancellationToken);

        public async Task<List<School>> GetAll(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<School>()
                .ToListAsync(cancellationToken);

        public async Task<List<School>> GetWithCurrentStudents(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<School>()
                .Where(school => school.Students.Any(student => !student.IsDeleted))
                .ToListAsync(cancellationToken);

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
                .Include(school => school.Staff)
                .Include(school => school.Staff)
                .ThenInclude(staff => staff.Faculties)
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

        public IList<MapLayer> GetForMapping(IList<string> schoolCodes)
        {
            var vm = new List<MapLayer>();

            var schools = _context.Schools
                .Include(school => school.Students)
                .Include(school => school.Staff);

            var filteredSchools = schoolCodes.Count > 0
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
