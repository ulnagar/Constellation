﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class FamilyRepository : IFamilyRepository
{
    private readonly AppDbContext _context;

    public FamilyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> DoesFamilyWithEmailExist(
        EmailAddress email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .AnyAsync(family => family.FamilyEmail == email.Email, cancellationToken);

    public async Task<Family> GetFamilyByEmail(
        EmailAddress email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .FirstOrDefaultAsync(family => family.FamilyEmail == email.Email, cancellationToken);

    public async Task<List<Family>> GetFamilyByMobileNumber(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Where(entry =>
                entry.Parents.Any(parent => 
                    parent.MobileNumber == phoneNumber.ToString(PhoneNumber.Format.None)))
            .ToListAsync(cancellationToken);

    public async Task<List<Family>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .ToListAsync(cancellationToken);

    public async Task<List<Family>> GetAllCurrent(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .Where(family => !family.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<Family?> GetFamilyBySentralId(
        string SentralId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .FirstOrDefaultAsync(family => family.SentralId == SentralId, cancellationToken);

    public async Task<Family?> GetFamilyById(
        FamilyId Id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .FirstOrDefaultAsync(family => family.Id == Id, cancellationToken);

    public async Task<List<Family>> GetFamiliesByStudentId(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .Include(family => family.Parents)
            .Include(family => family.Students)
            .Where(family => family.Students.Any(student => student.StudentId == studentId))
            .ToListAsync(cancellationToken);

    public async Task<bool> DoesEmailBelongToParentOrFamily(
        string email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Family>()
            .AnyAsync(family => 
                family.FamilyEmail.ToLower() == email.ToLower() ||
                family.Parents.Any(parent => parent.EmailAddress.ToLower() == email.ToLower()
            ), cancellationToken);

    public async Task<Dictionary<StudentId, bool>> GetStudentIdsFromFamilyWithEmail(
        string email,
        CancellationToken cancellation = default) =>
        await _context
            .Set<Family>()
            .Where(family =>
                family.FamilyEmail.ToLower() == email.ToLower() ||
                family.Parents.Any(parent => parent.EmailAddress.ToLower() == email.ToLower()))
            .SelectMany(family => family.Students)
            .ToDictionaryAsync(member => member.StudentId, member => member.IsResidentialFamily, cancellation);

    public async Task<int> CountOfParentsWithEmailAddress(
        string email,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Parent>()
            .CountAsync(parent => parent.EmailAddress.ToLower() == email.ToLower(), cancellationToken);

    public void Insert(Family family) =>
        _context.Set<Family>().Add(family);

    public void Remove(Parent parent) =>
        _context.Set<Parent>().Remove(parent);

    public void Remove(StudentFamilyMembership student) =>
        _context.Set<StudentFamilyMembership>().Remove(student);
}
