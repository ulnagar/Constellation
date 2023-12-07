namespace Constellation.Application.Training.Roles.DeleteTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteTrainingRoleCommandHandler
: ICommandHandler<DeleteTrainingRoleCommand>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DeleteTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<DeleteTrainingRoleCommand>();
    }

    public async Task<Result> Handle(DeleteTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(DeleteTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to delete the Training Role");

            return Result.Failure(TrainingErrors.Role.NotFound(request.RoleId));
        }

        role.Delete();

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
