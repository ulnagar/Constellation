namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal sealed class AbsenceResponseRepository : IAbsenceResponseRepository
{
    private readonly AppDbContext _context;

    public AbsenceResponseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Response>> GetAllForAbsence(
        AbsenceId absenceId, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Response>()
            .Where(response => response.AbsenceId == absenceId)
            .ToListAsync(cancellationToken);

    public async Task<Response> GetById(
        AbsenceResponseId responseId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Response>()
            .FirstOrDefaultAsync(response => response.Id == responseId, cancellationToken);

    public async Task<int> GetCountForAbsence(
        AbsenceId absenceId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Response>()
            .CountAsync(response => response.AbsenceId == absenceId, cancellationToken);
}
