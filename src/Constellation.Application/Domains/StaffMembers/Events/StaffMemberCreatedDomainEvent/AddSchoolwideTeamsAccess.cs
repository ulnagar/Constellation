namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
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
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddSchoolwideTeamsAccess(
        IStaffRepository staffRepository,
        ITeamOperationRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(StaffMemberCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Attempting to add staff member ({StaffId}) to school wide teams", notification.StaffId);

        StaffMember? staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

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

        ModifyTeamMembershipTeamOperation studentTeamOperation = new(
            MicrosoftTeam.StudentsTeamId,
            staffMember.EmailAddress,
            TeamAction.AddMember);
        
        _operationsRepository.Insert(studentTeamOperation);

        ModifyTeamChannelMembershipTeamOperation studentTeamChannelOperation = new(
            MicrosoftTeam.StudentsTeamId,
            $"{_dateTime.CurrentYear} - *",
            staffMember.EmailAddress,
            TeamAction.AddMember);

        _operationsRepository.Insert(studentTeamChannelOperation);

        ModifyTeamMembershipTeamOperation schoolTeamOperation = new(
            MicrosoftTeam.StaffTeamId,
            staffMember.EmailAddress,
            TeamAction.AddMember);
        
        _operationsRepository.Insert(schoolTeamOperation);

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled staff member ({StaffId}) addition to school wide teams", notification.StaffId);
    }
}
