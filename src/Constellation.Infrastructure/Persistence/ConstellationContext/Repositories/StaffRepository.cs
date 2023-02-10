using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
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
                    .ThenInclude(session => session.Room)
                .Include(staff => staff.Faculties)
                    .ThenInclude(member => member.Faculty);
        }

        public async Task<Staff?> GetById(
            string staffId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Staff>()
                .FirstOrDefaultAsync(staff => staff.StaffId == staffId, cancellationToken);

        public async Task<List<Staff>> GetListFromIds(
            List<string> staffIds,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Staff>()
                .Where(teacher => staffIds.Contains(teacher.StaffId))
                .ToListAsync(cancellationToken);

        public async Task<List<Staff>> GetCurrentTeachersForOffering(
            int offeringId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Staff>()
                .Where(teacher => teacher.CourseSessions.Any(session => session.OfferingId == offeringId && !session.IsDeleted))
                .ToListAsync(cancellationToken);

        public async Task<List<Staff>> GetFacultyHeadTeachers(
            Guid facultyId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Staff>()
                .Where(teacher => 
                    teacher.Faculties
                        .Any(faculty => 
                            faculty.FacultyId == facultyId && 
                            faculty.Role == FacultyMembershipRole.Manager && 
                            !faculty.IsDeleted))
                .ToListAsync(cancellationToken);

        public async Task<List<Staff>> GetFacultyHeadTeachersForOffering(
            int offeringId, 
            CancellationToken cancellationToken = default)
        {
            Guid? facultyId = await _context
                .Set<CourseOffering>()
                .Where(offering => offering.Id == offeringId)
                .Select(offering => offering.Course.FacultyId)
                .FirstOrDefaultAsync(cancellationToken);

            if (facultyId is null)
            {
                return null;
            }

            return await _context
                .Set<Staff>()
                .Where(staff => staff.Faculties.Any(member => 
                    !member.IsDeleted && 
                    member.FacultyId == facultyId && 
                    member.Role == FacultyMembershipRole.Manager))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Staff>> GetAllActive(
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Staff>()
                .Where(staff => !staff.IsDeleted)
                .ToListAsync(cancellationToken);

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
                .Include(staff => staff.Faculties)
                .ThenInclude(member => member.Faculty)
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

        public ICollection<Staff> AllFromFaculty(Guid facultyId)
        {
            return _context.Staff
                .Where(staff => staff.Faculties.Any(member => member.FacultyId == facultyId && !member.IsDeleted))
                .OrderBy(staff => staff.LastName)
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
                .FirstOrDefaultAsync(member => email.Contains(member.PortalUsername));
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
                .Include(staff => staff.Faculties)
                .ThenInclude(member => member.Faculty)
                .OrderBy(staff => staff.LastName)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Staff> ForDetailDisplayAsync(string id)
        {
            return await _context.Staff
                .Include(staff => staff.Faculties)
                .ThenInclude(member => member.Faculty)
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
                .Include(staff => staff.Faculties)
                .Include(staff => staff.CourseSessions)
                .Include(staff => staff.AdobeConnectGroupOperations)
                .SingleOrDefaultAsync(staff => staff.StaffId == id);
        }
    }
}
