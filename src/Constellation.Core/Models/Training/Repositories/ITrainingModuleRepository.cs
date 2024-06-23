namespace Constellation.Core.Models.Training.Repositories;

using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITrainingModuleRepository
{
    Task<List<TrainingModule>> GetAllModules(CancellationToken cancellationToken = default);
    Task<TrainingModule> GetModuleByName(string name, CancellationToken cancellationToken = default);
    Task<TrainingModule> GetModuleById(TrainingModuleId moduleId, CancellationToken cancellationToken = default);
    Task<List<TrainingModule>> GetModulesByAssignee(string staffId, CancellationToken cancellationToken = default);
    
    void Insert(TrainingModule module);
}