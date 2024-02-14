namespace Constellation.Application.Training.Roles.GetTrainingRoleListForStaffMember;

using Abstractions.Messaging;
using Constellation.Application.Training.Models;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingRoleListForStaffMemberQueryHandler
    : IQueryHandler<GetTrainingRoleListForStaffMemberQuery, List<TrainingRoleResponse>>
{
    private readonly ITrainingRoleRepository _roleRepository;

    public GetTrainingRoleListForStaffMemberQueryHandler(
        ITrainingRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<List<TrainingRoleResponse>>> Handle(GetTrainingRoleListForStaffMemberQuery request, CancellationToken cancellationToken)
    {
        List<TrainingRoleResponse> response = new();

        List<TrainingRole> roles = await _roleRepository.GetRolesForStaffMember(request.StaffId, cancellationToken);

        foreach (TrainingRole role in roles)
        {
            response.Add(new(
                role.Id,
                role.Name,
                role.Members.Count));
        }

        return response;
    }
}
