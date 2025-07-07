namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers.Events;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeamsAccess
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeamsAccess(
        IMSTeamOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = notification.StaffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _operationsRepository.Insert(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
        {
            StaffId = notification.StaffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Remove,
            PermissionLevel = MSTeamOperationPermissionLevel.Member,
            DateScheduled = _dateTime.Now,
        };

        _operationsRepository.Insert(schoolTeamOperation);
        
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
