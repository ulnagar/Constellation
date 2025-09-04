namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Awards;
using Constellation.Core.Models.Awards.Identifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AwardNominationRepository
    : IAwardNominationRepository
{
    private readonly AppDbContext _context;

    public AwardNominationRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<NominationPeriod>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<NominationPeriod>()
            .ToListAsync(cancellationToken);

    public async Task<NominationPeriod> GetById(
        AwardNominationPeriodId periodId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<NominationPeriod>()
            .FirstOrDefaultAsync(period => period.Id == periodId, cancellationToken);

    public async Task<List<NominationPeriod>> GetCurrentAndFuture(
        CancellationToken cancellationToken = default)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.Today);

        return await _context
            .Set<NominationPeriod>()
            .Where(period => period.LockoutDate >= currentDate)
            .ToListAsync(cancellationToken);
    }

    public void Insert(
        NominationPeriod period) =>
        _context.Set<NominationPeriod>().Add(period);
}
