using Constellation.Core.Models.MandatoryTraining;
using Microsoft.EntityFrameworkCore;

namespace Constellation.Application.Interfaces.Repositories;

public interface IMandatoryTrainingSets
{
    DbSet<TrainingCompletion> CompletionRecords { get; }
    DbSet<TrainingModule> Modules { get; }
}