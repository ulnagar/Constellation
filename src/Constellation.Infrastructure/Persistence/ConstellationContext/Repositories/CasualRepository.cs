namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Casuals;
using Microsoft.EntityFrameworkCore;

public class CasualRepository : ICasualRepository
{
    private readonly AppDbContext _context;

    public CasualRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Casual?> GetById(
        Guid id, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Casual>()
            .FirstOrDefaultAsync(casual => casual.Id == id, cancellationToken);

    public async Task<List<Casual>> GetAllActive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Casual>()
            .Where(casual => !casual.IsDeleted)
            .ToListAsync(cancellationToken);

    public void Insert(Casual casual) =>
        _context.Set<Casual>().Add(casual);
}