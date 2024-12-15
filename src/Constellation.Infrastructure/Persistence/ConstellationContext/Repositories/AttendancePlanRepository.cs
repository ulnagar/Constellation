namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public sealed class AttendancePlanRepository : IAttendancePlanRepository
{
    private readonly AppDbContext _context;

    public AttendancePlanRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<AttendancePlan> GetById(
        AttendancePlanId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

    public async Task<List<AttendancePlan>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .ToListAsync(cancellationToken);

    public async Task<List<AttendancePlan>> GetForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan => plan.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendancePlan>> GetPendingForSchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.Status == AttendancePlanStatus.Pending &&
                plan.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public void Insert(AttendancePlan plan) => _context.Set<AttendancePlan>().Add(plan);
}
