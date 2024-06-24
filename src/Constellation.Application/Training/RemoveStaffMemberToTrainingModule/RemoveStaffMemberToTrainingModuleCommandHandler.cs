namespace Constellation.Application.Training.RemoveStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Constellation.Core.Models.Training;
using Constellation.Core.Models.Training.Errors;
using Core.Models;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStaffMemberToTrainingModuleCommandHandler
: ICommandHandler<RemoveStaffMemberToTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStaffMemberToTrainingModuleCommandHandler(
        ITrainingModuleRepository moduleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<RemoveStaffMemberToTrainingModuleCommand>();
    }

    public async Task<Result> Handle(RemoveStaffMemberToTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _moduleRepository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(RemoveStaffMemberToTrainingModuleCommand), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.ModuleId), true)
                .Warning("Failed to add Staff Member to Training Module");

            return Result.Failure(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        Result assignee = module.RemoveAssignee(request.StaffId);

        if (assignee.IsFailure)
        {
            _logger
                .ForContext(nameof(RemoveStaffMemberToTrainingModuleCommand), request, true)
                .ForContext(nameof(Staff.StaffId), request.StaffId, true)
                .ForContext(nameof(Error), assignee.Error, true)
                .Warning("Failed to add Staff Member to Training Module");

            return Result.Failure(assignee.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
