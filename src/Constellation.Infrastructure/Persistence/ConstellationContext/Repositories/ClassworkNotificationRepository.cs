namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using Microsoft.EntityFrameworkCore;

public class ClassworkNotificationRepository : IClassworkNotificationRepository
{
    private readonly AppDbContext _context;

    public ClassworkNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClassworkNotification?> GetById(
        ClassworkNotificationId notificationId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .Include(notification => notification.Absences)
            .FirstOrDefaultAsync(record => record.Id ==  notificationId, cancellationToken);

    public async Task<List<ClassworkNotification>> GetForTeacher(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .Include(notification => notification.Absences)
            .Where(notification =>
                notification.Teachers.Any(teacher => teacher.StaffId == staffId))
            .ToListAsync(cancellationToken);

    public async Task<List<ClassworkNotification>> GetForOfferingAndDate(
        int offeringId, 
        DateOnly absenceDate, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .Where(record =>
                record.OfferingId == offeringId &&
                record.AbsenceDate == absenceDate)
            .ToListAsync(cancellationToken);

    public async Task<List<ClassworkNotification>> GetOutstandingForStudent(
        string studentId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .Include(notification => notification.Absences)
            .Where(notification => 
                notification.Absences.Any(absence => absence.StudentId == studentId) &&
                !notification.CompletedAt.HasValue)
            .ToListAsync(cancellationToken);

    public async Task<List<ClassworkNotification>> GetOutstandingForTeacher(
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .Include(notification => notification.Absences)
            .Where(notification =>
                notification.Teachers.Any(teacher => teacher.StaffId == staffId) &&
                !notification.CompletedAt.HasValue)
            .ToListAsync(cancellationToken);

    public void Insert(ClassworkNotification record) =>
        _context.Set<ClassworkNotification>().Add(record);
}
