namespace Constellation.Application.Training.Roles.AddModuleToTrainingRole;

using Abstractions.Messaging;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Models.Training.Errors;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddModuleToTrainingRoleCommandHandler
: ICommandHandler<AddModuleToTrainingRoleCommand>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddModuleToTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddModuleToTrainingRoleCommand>();
    }

    public async Task<Result> Handle(AddModuleToTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(AddModuleToTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to add module to training role");

            return Result.Failure(TrainingErrors.Role.NotFound(request.RoleId));
        }

        foreach (TrainingModuleId moduleId in request.ModuleIds)
        {
            Result result = role.AddModule(moduleId);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(AddModuleToTrainingRoleCommand), request, true)
                    .ForContext(nameof(moduleId), moduleId)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to add module to training role");

                return Result.Failure(result.Error);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
