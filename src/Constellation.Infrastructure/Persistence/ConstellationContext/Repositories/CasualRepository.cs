namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Casuals;
using Constellation.Core.Models.Identifiers;
using Microsoft.EntityFrameworkCore;

public class CasualRepository : ICasualRepository
{
    private readonly AppDbContext _context;

    public CasualRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Casual?> GetById(
        CasualId id, 
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

    public async Task<List<Casual>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Casual>()
            .ToListAsync(cancellationToken);

    public async Task<List<Casual>> GetAllInactive(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Casual>()
            .Where(casual => casual.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<Casual>> GetWithoutAdobeConnectId(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Casual>()
            .Where(casual => string.IsNullOrWhiteSpace(casual.AdobeConnectId))
            .ToListAsync(cancellationToken);

    public void Insert(Casual casual) =>
    _context.Set<Casual>().Add(casual);
}