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
            .FirstOrDefaultAsync(record => record.Id ==  notificationId, cancellationToken);

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

    public void Insert(ClassworkNotification record) =>
        _context.Set<ClassworkNotification>().Add(record);
}
