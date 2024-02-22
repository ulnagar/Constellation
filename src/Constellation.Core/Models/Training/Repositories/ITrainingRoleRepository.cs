namespace Constellation.Core.Models.Training.Repositories;

using Contexts.Roles;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ITrainingRoleRepository
{
    Task<TrainingRole> GetRoleByName(string name, CancellationToken cancellationToken = default);
    Task<TrainingRole> GetRoleById(TrainingRoleId roleId, CancellationToken cancellationToken = default);
    Task<List<TrainingRole>> GetAllRoles(CancellationToken cancellationToken = default);
    Task<List<TrainingRole>> GetRolesForStaffMember(string staffId, CancellationToken cancellationToken = default);
    Task<List<TrainingRole>> GetRolesForModule(TrainingModuleId moduleId, CancellationToken cancellationToken = default);
    void Insert(TrainingRole role);
}