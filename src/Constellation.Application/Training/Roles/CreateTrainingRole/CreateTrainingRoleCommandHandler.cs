namespace Constellation.Application.Training.Roles.CreateTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateTrainingRoleCommandHandler
: ICommandHandler<CreateTrainingRoleCommand, TrainingRoleResponse>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateTrainingRoleCommand>();
    }

    public async Task<Result<TrainingRoleResponse>> Handle(CreateTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole existingRole = await _roleRepository.GetRoleByName(request.Name, cancellationToken);

        if (existingRole is not null && !existingRole.IsDeleted)
        {
            _logger
                .ForContext(nameof(CreateTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.AlreadyExists(request.Name), true)
                .Warning("Could not create Training Role");

            return Result.Failure<TrainingRoleResponse>(TrainingErrors.Role.AlreadyExists(request.Name));
        }

        if (existingRole is not null && existingRole.IsDeleted)
        {
            existingRole.Restore();
            await _unitOfWork.CompleteAsync(cancellationToken);

            return new TrainingRoleResponse(
                existingRole.Id,
                existingRole.Name,
                existingRole.Members.Count);
        }
        
        TrainingRole role = TrainingRole.Create(request.Name);

        _roleRepository.Insert(role);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return new TrainingRoleResponse(
            role.Id,
            role.Name,
            role.Members.Count);
    }
}
