namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Microsoft.EntityFrameworkCore;

public class StaffRepository : IStaffRepository
{
    private readonly AppDbContext _context;

    public StaffRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffMember>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .ToListAsync(cancellationToken);

    public async Task<StaffMember?> GetById(
        StaffId staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .FirstOrDefaultAsync(staff => staff.Id == staffId, cancellationToken);

    public async Task<StaffMember?> GetByEmployeeId(
        EmployeeId employeeId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .FirstOrDefaultAsync(member => member.EmployeeId == employeeId, cancellationToken);

    public async Task<List<StaffMember>> GetListFromIds(
        List<StaffId> staffIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(teacher => staffIds.Contains(teacher.Id))
            .ToListAsync(cancellationToken);

    public async Task<List<StaffMember>> GetCurrentTeachersForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<StaffId> staffIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Teachers)
            .Where(assignment => !assignment.IsDeleted)
            .Select(assignment => assignment.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<StaffMember>()
            .Where(staff => staffIds.Contains(staff.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StaffMember>> GetPrimaryTeachersForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<StaffId> staffIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Teachers)
            .Where(assignment =>
                assignment.Type == AssignmentType.ClassroomTeacher &&
                !assignment.IsDeleted)
            .Select(assignment => assignment.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<StaffMember>()
            .Where(staff => staffIds.Contains(staff.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StaffMember>> GetFacultyHeadTeachers(
        FacultyId facultyId,
        CancellationToken cancellationToken = default)
    {
        List<StaffId> staffIds = await _context
            .Set<Faculty>()
            .Where(faculty => faculty.Id == facultyId)
            .SelectMany(faculty => faculty.Members)
            .Where(membership =>
                membership.Role == FacultyMembershipRole.Manager &&
                !membership.IsDeleted)
            .Select(membership => membership.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<StaffMember>()
            .Where(staff => staffIds.Contains(staff.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StaffMember>> GetFacultyHeadTeachersForOffering(
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

        List<StaffId> staffIds = await _context
            .Set<Faculty>()
            .Where(faculty => facultyIds.Contains(faculty.Id))
            .SelectMany(faculty => faculty.Members)
            .Where(membership =>
                membership.Role == FacultyMembershipRole.Manager &&
                !membership.IsDeleted)
            .Select(membership => membership.StaffId)
            .ToListAsync(cancellationToken);

        return await _context
            .Set<StaffMember>()
            .Where(staff => staffIds.Contains(staff.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StaffMember>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff => !staff.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<StaffMember>> GetActiveFromSchool(
        string schoolCode, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff => 
                !staff.IsDeleted &&
                staff.CurrentAssignment != null &&
                staff.CurrentAssignment.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public async Task<int> GetCountCurrentStaffFromSchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff =>
                !staff.IsDeleted &&
                staff.CurrentAssignment != null &&
                staff.CurrentAssignment.SchoolCode == schoolCode)
            .CountAsync(cancellationToken);

    public async Task<List<StaffId>> GetAllActiveStaffIds(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff => !staff.IsDeleted)
            .Select(staff => staff.Id)
            .ToListAsync(cancellationToken);

    public async Task<StaffMember?> GetCurrentByEmailAddress(
        string emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff => 
                staff.EmailAddress.Email == emailAddress && 
                !staff.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<StaffMember?> GetAnyByEmailAddress(
        string emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<StaffMember>()
            .Where(staff =>
                staff.EmailAddress.Email == emailAddress)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<StaffMember> GetFromName(
        string name,
        CancellationToken cancellationToken = default)
    {
        string[] splitName = name.ToLowerInvariant().Trim().Split(' ');

        StaffMember straightMatch = await _context
            .Set<StaffMember>()
            .FirstOrDefaultAsync(staff => 
                (staff.Name.FirstName.Contains(splitName[0]) || staff.Name.PreferredName.Contains(splitName[0])) && 
                staff.Name.LastName.Contains(splitName[1]),
                cancellationToken);

        if (straightMatch is not null)
            return straightMatch;

        string username = name.ToLowerInvariant().Trim().Replace(' ', '.');

        return await _context
            .Set<StaffMember>()
            .SingleOrDefaultAsync(member => member.EmailAddress.Email.Contains(username), cancellationToken);
    }

    public void Insert(StaffMember member) => _context.Set<StaffMember>().Add(member);
}
