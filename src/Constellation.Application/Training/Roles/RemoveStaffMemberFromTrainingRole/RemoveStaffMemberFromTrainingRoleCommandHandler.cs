namespace Constellation.Application.Training.Roles.RemoveStaffMemberFromTrainingRole;

using Abstractions.Messaging;
using Constellation.Core.Models.Training.Contexts.Roles;
using Constellation.Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStaffMemberFromTrainingRoleCommandHandler
: ICommandHandler<RemoveStaffMemberFromTrainingRoleCommand>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStaffMemberFromTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveStaffMemberFromTrainingRoleCommand>();
    }

    public async Task<Result> Handle(RemoveStaffMemberFromTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(RemoveStaffMemberFromTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to remove staff member from training role");

            return Result.Failure(TrainingErrors.Role.NotFound(request.RoleId));
        }

        Result result = role.RemoveMember(request.StaffId);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveStaffMemberFromTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove staff member from training role");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
