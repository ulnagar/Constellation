namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MandatoryTraining;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITrainingModuleRepository
{
    Task<List<TrainingModule>> GetAllCurrent(CancellationToken cancellationToken = default);
    Task<TrainingModule?> GetByName(string name, CancellationToken cancellationToken = default);
    Task<TrainingModule?> GetById(TrainingModuleId id, CancellationToken cancellationToken = default);
    void Insert(TrainingModule module);
}
