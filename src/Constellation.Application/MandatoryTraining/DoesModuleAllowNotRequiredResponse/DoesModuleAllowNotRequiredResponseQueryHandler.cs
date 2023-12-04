namespace Constellation.Application.MandatoryTraining.DoesModuleAllowNotRequiredResponse;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Shared;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DoesModuleAllowNotRequiredResponseQueryHandler 
    : IQueryHandler<DoesModuleAllowNotRequiredResponseQuery, bool>
{
    private readonly ITrainingRoleRepository _trainingRoleRepository;

    public DoesModuleAllowNotRequiredResponseQueryHandler(
        ITrainingRoleRepository trainingRoleRepository)
    {
        _trainingRoleRepository = trainingRoleRepository;
    }

    public async Task<Result<bool>> Handle(DoesModuleAllowNotRequiredResponseQuery request, CancellationToken cancellationToken)
    {
        List<TrainingRole> roles = await _trainingRoleRepository.GetRolesForModule(request.ModuleId, cancellationToken);

        return roles.Any(role => role.Members.Any(member => member.StaffId == request.StaffId));
    }
}
