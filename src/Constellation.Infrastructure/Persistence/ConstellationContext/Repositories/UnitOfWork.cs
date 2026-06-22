namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Core.Primitives;

public class UnitOfWork : IUnitOfWork
{
    private readonly ConstellationDbContext _context;
    
    public UnitOfWork(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task AddIntegrationEvent(IIntegrationEvent integrationEvent) =>
        await _context.AddIntegrationEvent(integrationEvent);

    public async Task CompleteAsync(CancellationToken token = default) => await _context.SaveChangesAsync(token);

}
