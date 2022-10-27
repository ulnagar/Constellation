using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.ContextSets;

public class MandatoryTrainingSets : IMandatoryTrainingSets
{
    private readonly AppDbContext _context;

    public MandatoryTrainingSets(AppDbContext context)
    {
        _context = context;
    }

    public DbSet<TrainingModule> Modules => _context.MandatoryTraining_Modules;
    public DbSet<TrainingCompletion> CompletionRecords => _context.MandatoryTraining_CompletionRecords;

}
