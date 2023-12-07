namespace Constellation.Application.Training.Roles.GetTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTrainingRoleQueryHandler
: IQueryHandler<GetTrainingRoleQuery, TrainingRoleResponse>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ILogger _logger;

    public GetTrainingRoleQueryHandler(
        ITrainingRoleRepository roleRepository,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _logger = logger.ForContext<GetTrainingRoleQuery>();
    }

    public async Task<Result<TrainingRoleResponse>> Handle(GetTrainingRoleQuery request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(GetTrainingRoleQuery), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to retrieve Training Role details");

            return Result.Failure<TrainingRoleResponse>(TrainingErrors.Role.NotFound(request.RoleId));
        }

        return new TrainingRoleResponse(
            role.Id,
            role.Name,
            role.Members.Count);
    }
}
