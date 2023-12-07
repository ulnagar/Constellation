namespace Constellation.Application.Training.Roles.UpdateTrainingRole;

using Abstractions.Messaging;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateTrainingRoleCommandHandler
    : ICommandHandler<UpdateTrainingRoleCommand, TrainingRoleResponse>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateTrainingRoleCommand>();
    }

    public async Task<Result<TrainingRoleResponse>> Handle(UpdateTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(UpdateTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Could not update Training Role");

            return Result.Failure<TrainingRoleResponse>(TrainingErrors.Role.NotFound(request.RoleId));
        }

        role.UpdateName(request.Name);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new TrainingRoleResponse(
            role.Id,
            role.Name,
            role.Members.Count);
    }
}
