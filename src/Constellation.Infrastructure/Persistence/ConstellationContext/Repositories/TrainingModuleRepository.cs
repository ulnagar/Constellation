namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;
using System.Threading;

internal sealed class TrainingModuleRepository
    : ITrainingModuleRepository
{
    private readonly AppDbContext _context;

    public TrainingModuleRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TrainingModule>> GetCurrentModules(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingModule>()
            .Where(module => !module.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<TrainingModule?> GetWithCompletionByName(
        string name,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingModule>()
            .Include(module => module.Completions)
            .Where(module => module.Name == name)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<TrainingModule?> GetById(
        TrainingModuleId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TrainingModule>()
            .Where(module => module.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

    public void Insert(TrainingModule module) => 
        _context.Add(module);
}
