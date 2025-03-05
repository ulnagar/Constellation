namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Enums;
using Core.Abstractions.Clock;
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
    private readonly IDateTimeProvider _dateTime;

    public AttendancePlanRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
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

    public async Task<AttendancePlan?> GetCurrentApprovedForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.StudentId == studentId &&
                plan.Status == AttendancePlanStatus.Accepted)
            .OrderByDescending(plan => plan.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AttendancePlan>> GetPendingForSchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.Status == AttendancePlanStatus.Pending &&
                plan.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendancePlan>> GetForSchool(
        string schoolCode,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.SchoolCode == schoolCode)
            .ToListAsync(cancellationToken);

    public async Task<List<AttendancePlan>> GetRecentForSchoolAndGrade(
        string schoolCode,
        Grade grade,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.SchoolCode == schoolCode &&
                plan.Grade == grade &&
                plan.CreatedAt.Year == _dateTime.Today.Year &&
                (plan.Status == AttendancePlanStatus.Accepted || plan.Status == AttendancePlanStatus.Processing))
            .ToListAsync(cancellationToken);


    public async Task<int> GetCountOfPending(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.CreatedAt.Year == _dateTime.Today.Year &&
                plan.Status == AttendancePlanStatus.Pending)
            .CountAsync(cancellationToken);

    public async Task<int> GetCountOfProcessing(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AttendancePlan>()
            .Where(plan =>
                plan.CreatedAt.Year == _dateTime.Today.Year &&
                plan.Status == AttendancePlanStatus.Processing)
            .CountAsync(cancellationToken);

    public void Insert(AttendancePlan plan) => _context.Set<AttendancePlan>().Add(plan);
}
