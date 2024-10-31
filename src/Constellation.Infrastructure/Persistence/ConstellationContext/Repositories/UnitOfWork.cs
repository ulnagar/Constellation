namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Core.Primitives;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public UnitOfWork(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddIntegrationEvent(IIntegrationEvent integrationEvent) =>
        await _context.AddIntegrationEvent(integrationEvent);

    public async Task CompleteAsync(CancellationToken token) => await _context.SaveChangesAsync(token);
    public async Task CompleteAsync() => await _context.SaveChangesAsync();
}
