namespace Constellation.Application.Training.Roles.RemoveModuleFromTrainingRole;

using Abstractions.Messaging;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveModuleFromTrainingRoleCommandHandler
: ICommandHandler<RemoveModuleFromTrainingRoleCommand>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveModuleFromTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveModuleFromTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RemoveModuleFromTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to remove module from training role");

            return Result.Failure(TrainingErrors.Role.NotFound(request.RoleId));
        }

        Result result = role.RemoveModule(request.ModuleId);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveModuleFromTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove module from training role");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
