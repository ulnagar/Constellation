namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberReinstatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Enums;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers.Events;
using Core.ValueObjects;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddSchoolwideTeamsAccess
: IDomainEventHandler<StaffMemberReinstatedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddSchoolwideTeamsAccess(
        IStaffRepository staffRepository,
        ITeamOperationRepository operationsRepository,
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

        ModifyTeamMembershipTeamOperation schoolTeamOperation = new(
            MicrosoftTeam.StaffTeamId,
            staffMember.EmailAddress,
            TeamAction.AddMember);

        _operationsRepository.Insert(schoolTeamOperation);

        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.Information("Scheduled staff member ({StaffId}) addition to school wide teams", notification.StaffId);
    }
}
