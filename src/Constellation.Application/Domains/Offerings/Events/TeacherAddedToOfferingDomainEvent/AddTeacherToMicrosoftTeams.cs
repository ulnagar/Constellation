namespace Constellation.Application.Domains.Offerings.Events.TeacherAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Models.LinkedSystems;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers.Errors;
using Core.ValueObjects;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeacherToMicrosoftTeams
    : IDomainEventHandler<TeacherAddedToOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeacherToMicrosoftTeams(
        IOfferingRepository offeringRepository,
        ITeamOperationRepository operationsRepository,
        IStaffRepository staffRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _operationsRepository = operationsRepository;
        _staffRepository = staffRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<TeacherAddedToOfferingDomainEvent>();
    }

    public async Task Handle(TeacherAddedToOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        Offering? offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        TeacherAssignment? assignment = offering.Teachers.FirstOrDefault(assignment => assignment.Id == notification.AssignmentId);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeacherAssignmentErrors.NotFound(notification.AssignmentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        StaffMember? staffMember = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(assignment.StaffId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        if (staffMember.EmailAddress == EmailAddress.None)
            return;

        foreach (Resource resource in offering.Resources.Where(resource => resource.Type == ResourceType.MicrosoftTeam))
        {
            List<Team> teams = await _teamRepository.GetByName(resource.ResourceId, cancellationToken);

            if (teams.Count == 0)
            {
                _logger
                    .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), TeamErrors.NotFoundByName(resource.ResourceId))
                    .Error("Failed to complete the event handler");

                continue;
            }

            if (teams.Count > 1)
            {
                _logger
                    .ForContext(nameof(TeacherAddedToOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), TeamErrors.TooManyResults(resource.ResourceId))
                    .Error("Failed to complete the event handler");

                continue;
            }

            ModifyTeamMembershipTeamOperation operation = new(
                teams.First().Id,
                staffMember.EmailAddress,
                TeamAction.AddOwner);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
