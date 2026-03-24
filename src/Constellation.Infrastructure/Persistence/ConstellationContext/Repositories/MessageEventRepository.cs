namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class MessageEventRepository : IMessageEventRepository
{
    private readonly AppDbContext _context;

    public MessageEventRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MessageEvent>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageEvent>()
            .ToListAsync(cancellationToken);

    public async Task<List<MessageEvent>> GetEventSummaries(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageEvent>()
            .ToListAsync(cancellationToken);

    public async Task<MessageEvent?> GetEventById(
        EventId eventId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageEvent>()
            .FirstOrDefaultAsync(message => message.Id == eventId, cancellationToken);

    public async Task<List<QueuedMessage>> GetQueuedMessagesByEventId(
        EventId eventId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<QueuedMessage>()
            .Where(item => item.EventId == eventId)
            .ToListAsync(cancellationToken);

    public void Insert(MessageEvent message) =>
        _context.Set<MessageEvent>().Add(message);

    public void Insert(QueuedMessage item) =>
        _context.Set<QueuedMessage>().Add(item);

    public void Remove(QueuedMessage item) =>
        _context.Set<QueuedMessage>().Remove(item);
}
