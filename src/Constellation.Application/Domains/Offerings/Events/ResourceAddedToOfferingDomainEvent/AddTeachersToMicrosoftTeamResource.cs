namespace Constellation.Application.Domains.Offerings.Events.ResourceAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Core.Abstractions.Repositories;
using Core.Models.LinkedSystems;
using Core.Models.LinkedSystems.Errors;
using Core.Models.Offerings.Enums;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddTeachersToMicrosoftTeamResource
    : IDomainEventHandler<ResourceAddedToOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeachersToMicrosoftTeamResource(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        ITeamOperationRepository operationsRepository,
        ITeamRepository teamRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResourceAddedToOfferingDomainEvent>();
    }

    public async Task Handle(ResourceAddedToOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ResourceType != ResourceType.MicrosoftTeam)
            return;

        Offering? offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        MicrosoftTeamResource? resource = offering.Resources.FirstOrDefault(resource => resource.Id == notification.ResourceId) as MicrosoftTeamResource;

        if (resource is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), ResourceErrors.NotFound(notification.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<Team> teams = await _teamRepository.GetByName(resource.ResourceId, cancellationToken);

        if (teams.Count == 0)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), TeamErrors.NotFoundByName(resource.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        if (teams.Count > 1)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
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
                TeamAction.AddOwner);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}