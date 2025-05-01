namespace Constellation.Application.Domains.Training.Commands.AddStaffMemberToTrainingModule;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStaffMemberToTrainingModuleCommandHandler
: ICommandHandler<AddStaffMemberToTrainingModuleCommand>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStaffMemberToTrainingModuleCommandHandler(
        ITrainingModuleRepository moduleRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddStaffMemberToTrainingModuleCommand>();
    }

    public async Task<Result> Handle(AddStaffMemberToTrainingModuleCommand request, CancellationToken cancellationToken)
    {
        TrainingModule module = await _moduleRepository.GetModuleById(request.ModuleId, cancellationToken);

        if (module is null)
        {
            _logger
                .ForContext(nameof(AddStaffMemberToTrainingModuleCommand), request, true)
                .ForContext(nameof(Error), TrainingModuleErrors.NotFound(request.ModuleId), true)
                .Warning("Failed to add Staff Member to Training Module");

            return Result.Failure(TrainingModuleErrors.NotFound(request.ModuleId));
        }

        foreach (string staffId in request.StaffMemberIds)
        {
            Staff member = await _staffRepository.GetById(staffId, cancellationToken);

            if (member is null)
            {
                _logger
                    .ForContext(nameof(AddStaffMemberToTrainingModuleCommand), request, true)
                    .ForContext(nameof(Staff.StaffId), staffId, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(staffId), true)
                    .Warning("Failed to add Staff Member to Training Module");

                return Result.Failure(DomainErrors.Partners.Staff.NotFound(staffId));
            }

            Result assignee = module.AddAssignee(staffId);

            if (assignee.IsFailure)
            {
                _logger
                    .ForContext(nameof(AddStaffMemberToTrainingModuleCommand), request, true)
                    .ForContext(nameof(Staff.StaffId), staffId, true)
                    .ForContext(nameof(Error), assignee.Error, true)
                    .Warning("Failed to add Staff Member to Training Module");

                return Result.Failure(assignee.Error);
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
