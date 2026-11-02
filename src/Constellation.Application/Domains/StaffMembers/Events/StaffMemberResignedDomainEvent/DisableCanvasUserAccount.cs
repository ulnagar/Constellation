namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.Events;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DisableCanvasUserAccount
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DisableCanvasUserAccount(
        IStaffRepository staffRepository,
        ICanvasOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberResignedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to process Resigned Staff Member");

            return;
        }

        DeleteUserCanvasOperation operation = new(staffMember.EmployeeId.ToString());

        _operationsRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
