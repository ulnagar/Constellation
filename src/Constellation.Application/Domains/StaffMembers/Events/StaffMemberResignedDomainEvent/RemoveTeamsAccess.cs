namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeamsAccess
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeamsAccess(
        ITeamOperationRepository operationsRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<StaffMemberResignedDomainEvent>();
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

        ModifyTeamMembershipTeamOperation studentTeamOperation = new(
            MicrosoftTeam.StudentsTeamId,
            staffMember.EmailAddress,
            TeamAction.Remove);

        _operationsRepository.Insert(studentTeamOperation);

        ModifyTeamMembershipTeamOperation schoolTeamOperation = new(
            MicrosoftTeam.StaffTeamId,
            staffMember.EmailAddress,
            TeamAction.Remove);

        _operationsRepository.Insert(schoolTeamOperation);
        
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
