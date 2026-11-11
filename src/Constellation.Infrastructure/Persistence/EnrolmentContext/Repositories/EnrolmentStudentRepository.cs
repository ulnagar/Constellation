namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Repositories;

using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.EnrolmentContext.EnrolmentStudent.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentStudent.Repositories;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

internal sealed class EnrolmentStudentRepository : IEnrolmentStudentRepository
{
    private readonly EnrolmentDbContext _context;

    public EnrolmentStudentRepository(
        EnrolmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<EnrolmentStudent>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .ToListAsync(cancellationToken);

    public async Task<EnrolmentStudent?> GetById(
        EnrolmentStudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

    public async Task<EnrolmentStudent?> GetByEmail(
        EmailAddress email, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);

    public async Task<EnrolmentStudent?> GetBySRN(
        StudentReferenceNumber srn, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .FirstOrDefaultAsync(s => s.StudentReferenceNumber == srn, cancellationToken);

    public async Task<List<EnrolmentStudent>> GetByName(
        string firstName, 
        string lastName, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .Where(s => s.Name.FirstName == firstName && s.Name.LastName == lastName)
            .ToListAsync(cancellationToken);

    public async Task<List<EnrolmentStudent>> GetListFromIds(
        List<EnrolmentStudentId> studentIds,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EnrolmentStudent>()
            .Where(s => studentIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

    public void Insert(EnrolmentStudent student) => _context.Set<EnrolmentStudent>().Add(student);

    public void Remove(EnrolmentStudent student) => _context.Set<EnrolmentStudent>().Remove(student);
}
