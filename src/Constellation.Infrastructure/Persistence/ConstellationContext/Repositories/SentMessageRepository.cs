namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class SentMessageRepository : ISentMessageRepository
{
    private readonly AppDbContext _context;

    public SentMessageRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SentMessage>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SentMessage>()
            .ToListAsync(cancellationToken);

    public async Task<List<SentMessage>> GetMessageSummaries(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SentMessage>()
            .IgnoreAutoIncludes()
            .ToListAsync(cancellationToken);

    public async Task<SentMessage?> GetMessageById(
        EventId eventId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SentMessage>()
            .FirstOrDefaultAsync(message => message.Id == eventId, cancellationToken);

    public void Insert(SentMessage message) =>
        _context.Set<SentMessage>().Add(message);
}
