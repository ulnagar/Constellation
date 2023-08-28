namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.Subjects.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class EnrolmentRepository : IEnrolmentRepository
{
    private readonly AppDbContext _context;

    public EnrolmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Enrolment>> GetCurrentByStudentId(
        string studentId,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;

        return await _context
            .Set<Enrolment>()
            .Include(enrol => enrol.Offering)
            .ThenInclude(offering => offering.Course)
            .Where(enrol =>
                enrol.StudentId == studentId &&
                !enrol.IsDeleted &&
                enrol.Offering.EndDate >= today &&
                enrol.Offering.StartDate <= today)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Enrolment>> GetCurrentByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Enrolment>()
            .Where(enrol => enrol.OfferingId == offeringId && !enrol.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<Enrolment> ForEditing(int id)
    {
        return await _context.Enrolments
            .Include(enrolment => enrolment.Student)
            .Include(enrolment => enrolment.Offering)
            .ThenInclude(offering => offering.Sessions)
            .ThenInclude(session => session.Room)
            .SingleOrDefaultAsync(enrolment => enrolment.Id == id);
    }

    public async Task<bool> AnyForStudentAndOffering(string studentId, OfferingId offeringId)
    {
        return await _context.Enrolments
            .AnyAsync(enrolment => !enrolment.IsDeleted && enrolment.StudentId == studentId && enrolment.OfferingId == offeringId);
    }
}