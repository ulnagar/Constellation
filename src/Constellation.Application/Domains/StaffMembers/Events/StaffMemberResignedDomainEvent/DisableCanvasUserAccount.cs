namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Core.Models.StaffMembers.Events;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DisableCanvasUserAccount
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DisableCanvasUserAccount(
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        DeleteUserCanvasOperation operation = new(notification.StaffId.ToString());

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
