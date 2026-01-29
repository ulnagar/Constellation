namespace Constellation.Application.Domains.Offerings.Events.ResourceRemovedFromOfferingDomainEvent;

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
using Constellation.Core.Shared;
using Core.Models.LinkedSystems;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveTeachersFromMicrosoftTeamResource
    : IDomainEventHandler<ResourceRemovedFromOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeachersFromMicrosoftTeamResource(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ITeamOperationRepository operationsRepository,
        ITeamRepository teamRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResourceRemovedFromOfferingDomainEvent>();
    }

    public async Task Handle(ResourceRemovedFromOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Resource.Type != ResourceType.MicrosoftTeam)
            return;

        Offering? offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        MicrosoftTeamResource? resource = notification.Resource as MicrosoftTeamResource;
        
        List<Team> teams = await _teamRepository.GetByName(resource.ResourceId, cancellationToken);

        if (teams.Count == 0)
        {
            _logger
                .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.NotFoundByName(resource.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        if (teams.Count > 1)
        {
            _logger
                .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.TooManyResults(resource.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<StaffId> staffIds = offering.Teachers.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.StaffId).ToList();

        List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

        foreach (StaffMember staffMember in staffMembers)
        {
            ModifyTeamMembershipTeamOperation operation = new(
                teams.First().Id,
                staffMember.EmailAddress,
                TeamAction.Remove);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}