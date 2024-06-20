namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class StaffRepository : IStaffRepository
{
    private readonly AppDbContext _context;

    public StaffRepository(
        AppDbContext context)
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
            .Include(staff => staff.Faculties);
    }

    public async Task<List<Staff>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Include(member => member.Faculties)
            .ToListAsync(cancellationToken);

    public async Task<Staff?> GetById(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Include(staff => staff.Faculties)
            .FirstOrDefaultAsync(staff => staff.StaffId == staffId, cancellationToken);

    public async Task<List<Staff>> GetListFromIds(
        List<string> staffIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Where(teacher => staffIds.Contains(teacher.StaffId))
            .ToListAsync(cancellationToken);

    public async Task<List<Staff>> GetCurrentTeachersForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> staffIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Teachers)
            .Where(assignment => !assignment.IsDeleted)
            .Select(assignment => assignment.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Staff>()
            .Where(staff => staffIds.Contains(staff.StaffId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Staff>> GetPrimaryTeachersForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> staffIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Teachers)
            .Where(assignment =>
                assignment.Type == AssignmentType.ClassroomTeacher &&
                !assignment.IsDeleted)
            .Select(assignment => assignment.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Staff>()
            .Where(staff => staffIds.Contains(staff.StaffId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Staff>> GetFacultyHeadTeachers(
        FacultyId facultyId,
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
        OfferingId offeringId, 
        CancellationToken cancellationToken = default)
    {
        List<CourseId> courseIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .Select(offering => offering.CourseId)
            .ToListAsync(cancellationToken);

        List<FacultyId> facultyIds = await _context
            .Set<Course>()
            .Where(course => courseIds.Contains(course.Id))
            .Select(course => course.FacultyId)
            .ToListAsync(cancellationToken);

        List<string> staffIds = await _context
            .Set<Faculty>()
            .Where(faculty => facultyIds.Contains(faculty.Id))
            .SelectMany(faculty => faculty.Members)
            .Where(member => 
                !member.IsDeleted &&
                member.Role == FacultyMembershipRole.Manager)
            .Select(member => member.StaffId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<Staff>()
            .Where(staff => staffIds.Contains(staff.StaffId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Staff>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Include(staff => staff.Faculties)
            .Where(staff => !staff.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Staff>> GetActiveFromSchool(
        string schoolCode, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Where(staff => 
                !staff.IsDeleted &&
                staff.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetAllActiveStaffIds(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Where(staff => !staff.IsDeleted)
            .Select(staff => staff.StaffId)
            .ToListAsync(cancellationToken);

    public async Task<Staff?> GetCurrentByEmailAddress(
        string emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Where(staff => 
                emailAddress.Contains(staff.PortalUsername) && 
                !staff.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Staff?> GetAnyByEmailAddress(
        string emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Staff>()
            .Where(staff =>
                emailAddress.Contains(staff.PortalUsername))
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(Staff member) => _context.Set<Staff>().Add(member);
        
    public Staff WithDetails(string id)
    {
        return Collection()
            .SingleOrDefault(d => d.StaffId == id);
    }

    public async Task<Staff> GetForExistCheck(string id)
    {
        return await _context.Staff
            .SingleOrDefaultAsync(staff => staff.StaffId == id);
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
        string[] splitName = name.ToLowerInvariant().Trim().Split(' ');

        Staff straightMatch = await _context
            .Set<Staff>()
            .FirstOrDefaultAsync(staff => staff.FirstName.Contains(splitName[0]) && staff.LastName.Contains(splitName[1]));

        if (straightMatch is not null)
            return straightMatch;

        string username = name.ToLowerInvariant().Trim().Replace(' ', '.');

        return await _context.Staff
            .SingleOrDefaultAsync(member => member.PortalUsername.Contains(username));
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
            .OrderBy(staff => staff.LastName)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Staff> ForDetailDisplayAsync(string id)
    {
        return await _context.Staff
            .Include(staff => staff.Faculties)
            .Include(staff => staff.School)
            .ThenInclude(school => school.StaffAssignments)
            .SingleOrDefaultAsync(staff => staff.StaffId == id);
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
            .Include(staff => staff.AdobeConnectGroupOperations)
            .SingleOrDefaultAsync(staff => staff.StaffId == id);
    }
}
