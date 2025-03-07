namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Enums;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students;
using Microsoft.EntityFrameworkCore;

public class SchoolContactRepository : ISchoolContactRepository
{
    private readonly AppDbContext _context;

    public SchoolContactRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Insert(SchoolContact contact) => _context.Set<SchoolContact>().Add(contact);

    public async Task<List<SchoolContact>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetAllWithRole(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(contact => contact.Assignments.Any(assignment =>
                !assignment.IsDeleted))
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetAllWithoutRole(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(contact => 
                contact.Assignments.Count == 0 ||
                contact.Assignments.All(assignment => assignment.IsDeleted))
            .ToListAsync(cancellationToken);
    
    public async Task<SchoolContact?> GetById(
        SchoolContactId contactId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .SingleOrDefaultAsync(entry => entry.Id == contactId, cancellationToken);

    public async Task<SchoolContact> GetByNameAndSchool(
        string name,
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(entry => 
                name.Contains(entry.FirstName) && 
                name.Contains(entry.LastName) &&
                entry.Assignments.Any(role =>
                    !role.IsDeleted &&
                    role.SchoolCode == schoolCode))
            .SingleOrDefaultAsync(cancellationToken);
    
    public async Task<List<SchoolContact>> GetPrincipalsForSchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(contact =>
                contact.Assignments.Any(role =>
                    !role.IsDeleted &&
                    role.Role == SchoolContactRole.Principal &&
                    role.SchoolCode == schoolCode))
            .ToListAsync(cancellationToken);

    public async Task<SchoolContact?> GetWithRolesByEmailAddress(
        string emailAddress,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Include(contact => contact.Assignments.Where(role => !role.IsDeleted))
            .Where(contact => contact.EmailAddress == emailAddress)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetWithRolesBySchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(contact => contact.Assignments.Any(role => 
                !role.IsDeleted && 
                role.SchoolCode == schoolCode))
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetByGrade(
        Grade grade,
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        List<string> schoolCodes = await _context
            .Set<Student>()
            .Where(student => !student.IsDeleted)
            .SelectMany(student => student.SchoolEnrolments.Where(enrolment => 
                !enrolment.IsDeleted &&
                enrolment.StartDate <= today &&
                (!enrolment.EndDate.HasValue || enrolment.EndDate >= today) &&
                enrolment.Grade == grade))
            .Select(enrolment => enrolment.SchoolCode)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<SchoolContact>()
            .Where(contact => contact.Assignments.Any(role =>
                !role.IsDeleted &&
                schoolCodes.Contains(role.SchoolCode)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SchoolContact>> GetBySchoolAndRole(
        string schoolCode,
        string selectedRole,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Include(contact => contact.Assignments.Where(role => !role.IsDeleted))
            .Where(contact => 
                contact.Assignments.Any(role => 
                    !role.IsDeleted && 
                    role.SchoolCode == schoolCode && 
                    role.Role == selectedRole))
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetAllByRole(
        string selectedRole,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Include(contact => contact.Assignments.Where(role => !role.IsDeleted))
            .Where(contact => 
                contact.Assignments.Any(role =>
                    !role.IsDeleted &&
                    role.Role == selectedRole))
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetAvailableRoleList(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContactRole>()
            .Select(role => role.Role)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SchoolContact>()
            .Where(contact => 
                !contact.IsDeleted &&
                contact.Assignments.Any(role => !role.IsDeleted))
            .ToListAsync(cancellationToken);
}