namespace Constellation.Application.Domains.Offerings.Events.TeacherRemovedFromOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Operations;
using Constellation.Core.Models.Operations.Enums;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Core.Models.LinkedSystems;
using Core.Models.Operations.Repositories;
using Core.ValueObjects;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeacherFromMicrosoftTeams
    : IDomainEventHandler<TeacherRemovedFromOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeacherFromMicrosoftTeams(
        IOfferingRepository offeringRepository,
        ITeamOperationRepository operationsRepository,
        ITeamRepository teamRepository,
        IStaffRepository staffRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<TeacherRemovedFromOfferingDomainEvent>();
    }

    public async Task Handle(TeacherRemovedFromOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        Offering? offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        TeacherAssignment? assignment = offering.Teachers.FirstOrDefault(assignment => assignment.Id == notification.AssignmentId);

        if (assignment is null)
        {
            _logger
                .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeacherAssignmentErrors.NotFound(notification.AssignmentId))
                .Error("Failed to complete the event handler");

            return;
        }

        StaffMember? staffMember = await _staffRepository.GetById(assignment.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
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
                    .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), TeamErrors.NotFoundByName(resource.ResourceId))
                    .Error("Failed to complete the event handler");

                continue;
            }

            if (teams.Count > 1)
            {
                _logger
                    .ForContext(nameof(TeacherRemovedFromOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), TeamErrors.TooManyResults(resource.ResourceId))
                    .Error("Failed to complete the event handler");

                continue;
            }

            ModifyTeamMembershipTeamOperation operation = new(
                teams.First().Id,
                staffMember.EmailAddress,
                TeamAction.Remove);

            _operationsRepository.Insert(operation);
        }
        
        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
