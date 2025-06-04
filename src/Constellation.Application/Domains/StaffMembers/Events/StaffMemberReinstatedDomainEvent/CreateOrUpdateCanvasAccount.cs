namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberReinstatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.StaffMembers.ValueObjects;
using Core.Models.StaffMembers.Events;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateCanvasAccount
: IDomainEventHandler<StaffMemberReinstatedDomainEvent>
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

    public async Task Handle(StaffMemberReinstatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add staff member ({StaffId}) to Canvas", notification.StaffId);

        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger.Warning("Could not find Staff Member with Id {StaffId} to add to Canvas", notification.StaffId);
            return;
        }

        if (staffMember.EmployeeId == EmployeeId.Empty)
        {
            _logger.Warning("Staff Member with id {StaffId} does not have a valid Employee ID to add to Canvas", notification.StaffId);
            return;
        }

        if (staffMember.EmailAddress == EmailAddress.None)
        {
            _logger.Warning("Staff Member with id {StaffId} does not have a valid email address to add to Canvas", notification.StaffId);
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

        _logger.Information("Scheduled Staff Member ({StaffId}) addition to Canvas", notification.StaffId);
    }
}
