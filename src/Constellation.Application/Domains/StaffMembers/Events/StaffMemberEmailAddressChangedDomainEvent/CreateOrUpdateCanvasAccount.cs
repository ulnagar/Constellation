namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberEmailAddressChangedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Events;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateCanvasAccount
: IDomainEventHandler<StaffMemberEmailAddressChangedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ICanvasOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateOrUpdateCanvasAccount(
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

    public async Task Handle(StaffMemberEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to update staff member ({StaffId}) email in Canvas", notification.StaffId);
        
        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to update staff member ({StaffId}) email in Canvas", notification.StaffId);

            return;
        }

        if (staffMember.EmployeeId == EmployeeId.Empty)
        {
            _logger
                .ForContext(nameof(StaffMember), staffMember, true)
                .Warning("Failed to update staff member ({StaffId}) email in Canvas", notification.StaffId);

            return;
        }

        if (staffMember.EmailAddress == EmailAddress.None)
        {
            _logger
                .ForContext(nameof(StaffMember), staffMember, true)
                .Warning("Failed to update staff member ({StaffId}) email in Canvas", notification.StaffId);
            return;
        }

        CreateUserCanvasOperation operation = new(
            staffMember.EmployeeId.Number,
            staffMember.Name.FirstName,
            staffMember.Name.LastName,
            staffMember.EmailAddress.Email.Substring(0, staffMember.EmailAddress.Email.IndexOf('@')),
            staffMember.EmailAddress.Email);

        _operationsRepository.Insert(operation);
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled Staff Member ({StaffId}) email update in Canvas", notification.StaffId);
    }
}
