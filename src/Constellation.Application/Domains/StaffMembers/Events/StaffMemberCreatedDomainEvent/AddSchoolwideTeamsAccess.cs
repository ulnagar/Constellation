namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Core.Abstractions.Clock;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Repositories;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSchoolwideTeamsAccess
: IDomainEventHandler<StaffMemberCreatedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public AddSchoolwideTeamsAccess(
        IStaffRepository staffRepository,
        IMSTeamOperationsRepository operationsRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task Handle(StaffMemberCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add staff member ({StaffId}) to school wide teams", notification.StaffId);

        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger.Warning("Could not find staff member with Id {StaffId} to add to school wide teams", notification.StaffId);
            return;
        }

        if (staffMember.EmailAddress == EmailAddress.None)
        {
            _logger.Warning("Staff member with id {StaffId} does not have a valid email address to add to school wide teams", notification.StaffId);
            return;
        }

        TeacherEmployedMSTeamOperation studentTeamOperation = new()
        {
            StaffId = notification.StaffId,
            TeamName = MicrosoftTeam.Students,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationsRepository.Insert(studentTeamOperation);

        TeacherEmployedMSTeamOperation schoolTeamOperation = new()
        {
            StaffId = notification.StaffId,
            TeamName = MicrosoftTeam.Staff,
            Action = MSTeamOperationAction.Add,
            DateScheduled = _dateTime.Now,
            PermissionLevel = MSTeamOperationPermissionLevel.Member
        };

        _operationsRepository.Insert(schoolTeamOperation);

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled staff member ({StaffId}) addition to school wide teams", notification.StaffId);
    }
}
