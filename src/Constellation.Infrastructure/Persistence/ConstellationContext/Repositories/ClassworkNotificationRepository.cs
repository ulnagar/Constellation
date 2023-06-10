﻿namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

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
            .FirstOrDefaultAsync(record => record.Id ==  notificationId, cancellationToken);

    public async Task<ClassworkNotification?> GetForOfferingAndDate(
        int offeringId, 
        DateOnly absenceDate, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<ClassworkNotification>()
            .FirstOrDefaultAsync(record =>
                record.OfferingId == offeringId &&
                record.AbsenceDate == absenceDate,
            cancellationToken);


    public void Insert(ClassworkNotification record) =>
        _context.Set<ClassworkNotification>().Add(record);

    public async Task<ClassworkNotification> Get(Guid id, CancellationToken token = default)
    {
        return await _context.ClassworkNotifications
            .Include(notification => notification.Absences)
            .ThenInclude(absence => absence.Student)
            .Include(notification => notification.Offering)
            .Include(notification => notification.Offering.Course)
            .Include(notification => notification.Teachers)
            .SingleOrDefaultAsync(notification => notification.Id == id, token);
    }

    public async Task<ICollection<ClassworkNotification>> GetAll(CancellationToken token = default)
    {
        return await _context.ClassworkNotifications
            .Include(notification => notification.Absences)
            .Include(notification => notification.Offering)
            .Include(notification => notification.Teachers)
            .ToListAsync(token);
    }

    public async Task<ClassworkNotification> GetForDuplicateCheck(int offeringId, DateTime absenceDate, CancellationToken token = default)
    {
        return await _context.ClassworkNotifications
            .Include(notification => notification.Absences)
            .Include(notification => notification.Offering)
            .Include(notification => notification.Teachers)
            .SingleOrDefaultAsync(notification => notification.AbsenceDate == absenceDate && notification.OfferingId == offeringId, token);
    }

    public async Task<ICollection<ClassworkNotification>> GetOutstandingForTeacher(string staffId, CancellationToken token = default)
    {
        return await _context.ClassworkNotifications
            .Include(notification => notification.Absences)
            .ThenInclude(absence => absence.Student)
            .Include(notification => notification.Offering)
            .Include(notification => notification.Offering.Course)
            .Where(notification => notification.Teachers.Any(teacher => teacher.StaffId == staffId) && !notification.CompletedAt.HasValue)
            .ToListAsync(token);
    }

    public async Task<ICollection<ClassworkNotification>> GetForTeacher(string staffId, CancellationToken token = default)
    {
        return await _context.ClassworkNotifications
            .Include(notification => notification.Offering)
            .Include(notification => notification.Absences)
            .ThenInclude(absence => absence.Student)
            .Where(notification => notification.Teachers.Any(staff => staff.StaffId == staffId))
            .ToListAsync(token);
    }
}
