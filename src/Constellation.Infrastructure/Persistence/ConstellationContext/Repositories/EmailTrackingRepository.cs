namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.Messaging.Tracking.Repositories;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

internal sealed class EmailTrackingRepository : IEmailTrackingRepository
{
    private readonly ConstellationDbContext _context;

    public EmailTrackingRepository(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmailTrackingEvent>> GetTrackingEventsByEmailId(
        EmailId emailId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<EmailTrackingEvent>()
            .Where(tracker => tracker.EmailId == emailId)
            .ToListAsync(cancellationToken);
}
