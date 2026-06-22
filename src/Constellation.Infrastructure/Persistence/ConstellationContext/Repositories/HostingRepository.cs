namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Hosting;
using Constellation.Core.Models.Hosting.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class HostingRepository : IHostingRepository
{
    private readonly ConstellationDbContext _context;

    public HostingRepository(ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Newsletter>> GetAllNewsletters(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Newsletter>()
            .OrderByDescending(newsletter => newsletter.Issue)
            .ToListAsync(cancellationToken);

    public async Task<Newsletter?> GetNewsletterByIssue(
        int issue,
        CancellationToken cancellationToken = default) =>
        await _context
        .Set<Newsletter>()
        .FirstOrDefaultAsync(newsletter => newsletter.Issue == issue, cancellationToken);

    public async Task<List<Livestream>> GetAllLivestreams(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Livestream>()
            .OrderByDescending(livestream => livestream.StartsOn)
            .ToListAsync(cancellationToken);

    public async Task<Livestream?> GetLivestreamById(
        Guid id, 
        CancellationToken cancellationToken = default) =>
    await _context
        .Set<Livestream>()
        .FirstOrDefaultAsync(livestream => livestream.Id == id, cancellationToken);

    public void Insert(Newsletter newsletter) => _context.Set<Newsletter>().Add(newsletter);
    public void Insert(Livestream livestream) => _context.Set<Livestream>().Add(livestream);
}
