namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

internal sealed class AbsenceNotificationRepository : IAbsenceNotificationRepository
{
    private readonly AppDbContext _context;

    public AbsenceNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCountForAbsence(
        AbsenceId absenceId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Notification>()
            .CountAsync(notification => notification.AbsenceId == absenceId, cancellationToken);
}