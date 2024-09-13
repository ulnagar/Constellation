namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    
    public UnitOfWork(
        AppDbContext context)
    {
        _context = context;
    }
    
    public async Task CompleteAsync(CancellationToken token) => await _context.SaveChangesAsync(token);

    public async Task CompleteAsync() => await _context.SaveChangesAsync();
}
