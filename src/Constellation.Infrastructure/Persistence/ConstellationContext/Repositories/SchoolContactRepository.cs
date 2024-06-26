﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Enums;
using Core.Models;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.SchoolContacts.Repositories;
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
            .Include(contact => contact.Assignments.Where(role => !role.IsDeleted))
            .Where(contact => contact.Assignments.Any(role => !role.IsDeleted && role.SchoolCode == schoolCode))
            .ToListAsync(cancellationToken);

    public async Task<List<SchoolContact>> GetByGrade(
        Grade grade,
        CancellationToken cancellationToken = default)
    {
        List<string> schoolCodes = await _context
            .Set<School>()
            .Where(school =>
                school.Students
                    .Any(student =>
                        !student.IsDeleted &&
                        student.CurrentGrade == grade))
            .Select(school => school.Code)
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