namespace Constellation.Application.Training.Roles.AddStaffMemberToTrainingRole;

using Abstractions.Messaging;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStaffMemberToTrainingRoleCommandHandler
: ICommandHandler<AddStaffMemberToTrainingRoleCommand>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStaffMemberToTrainingRoleCommandHandler(
        ITrainingRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddStaffMemberToTrainingRoleCommand>();
    }

    public async Task<Result> Handle(AddStaffMemberToTrainingRoleCommand request, CancellationToken cancellationToken)
    {
        TrainingRole role = await _roleRepository.GetRoleById(request.RoleId, cancellationToken);

        if (role is null)
        {
            _logger
                .ForContext(nameof(AddStaffMemberToTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), TrainingErrors.Role.NotFound(request.RoleId), true)
                .Warning("Failed to add staff member to training role");

            return Result.Failure(TrainingErrors.Role.NotFound(request.RoleId));
        }

        Result result = role.AddMember(request.StaffId);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(AddStaffMemberToTrainingRoleCommand), request, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add staff member to training role");

            return Result.Failure(result.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return result;
    }
}
