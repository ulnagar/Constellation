namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class MessageTemplateRepository : IMessageTemplateRepository
{
    private readonly ConstellationDbContext _context;

    public MessageTemplateRepository(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MessageTemplate>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageTemplate>()
            .ToListAsync(cancellationToken);

    public async Task<MessageTemplate> GetById(
        TemplateId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageTemplate>()
            .FirstOrDefaultAsync(entry => entry.Id == id, cancellationToken);

    public async Task<MessageTemplate?> GetByName(
        string name,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<MessageTemplate>()
            .FirstOrDefaultAsync(entry => entry.Name == name, cancellationToken);

    public void Insert(MessageTemplate template) => _context.Set<MessageTemplate>().Add(template);

    public void Remove(MessageTemplate template) => _context.Set<MessageTemplate>().Remove(template);
}
