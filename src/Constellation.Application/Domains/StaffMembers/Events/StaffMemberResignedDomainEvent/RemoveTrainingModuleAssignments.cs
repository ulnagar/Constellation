namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Models.Training;
using Constellation.Core.Shared;
using Core.Models.StaffMembers.Events;
using Core.Models.Training.Repositories;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTrainingModuleAssignments
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTrainingModuleAssignments(
        ITrainingModuleRepository moduleRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        List<TrainingModule> modules = await _moduleRepository.GetModulesByAssignee(notification.StaffId, cancellationToken);

        foreach (TrainingModule module in modules)
        {
            Result result = module.RemoveAssignee(notification.StaffId);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(StaffMemberResignedDomainEvent), notification, true)
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to mark Staff Member resigned");
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
