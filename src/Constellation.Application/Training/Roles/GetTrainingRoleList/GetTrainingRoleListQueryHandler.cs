namespace Constellation.Application.Training.Roles.GetTrainingRoleList;

using Abstractions.Messaging;
using Constellation.Application.Training.Models;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingRoleListQueryHandler
: IQueryHandler<GetTrainingRoleListQuery, List<TrainingRoleResponse>>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public GetTrainingRoleListQueryHandler(
        ITrainingRoleRepository roleRepository,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _logger = logger.ForContext<GetTrainingRoleListQuery>();
    }

    public async Task<Result<List<TrainingRoleResponse>>> Handle(GetTrainingRoleListQuery request, CancellationToken cancellationToken)
    {
        List<TrainingRoleResponse> result = new();

        List<TrainingRole> roles = await _roleRepository.GetAllRoles(cancellationToken);

        foreach (TrainingRole role in roles)
        {
            if (role.IsDeleted) continue;

            result.Add(new(
                role.Id,
                role.Name,
                role.Members.Count));
        }

        return result;
    }
}
