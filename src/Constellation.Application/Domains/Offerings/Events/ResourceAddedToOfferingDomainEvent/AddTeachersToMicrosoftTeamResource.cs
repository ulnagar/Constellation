namespace Constellation.Application.Domains.Offerings.Events.ResourceAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
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
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddTeachersToMicrosoftTeamResource(
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IMSTeamOperationsRepository operationsRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _operationsRepository = operationsRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResourceAddedToOfferingDomainEvent>();
    }

    public async Task Handle(ResourceAddedToOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ResourceType != ResourceType.MicrosoftTeam)
            return;

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        MicrosoftTeamResource resource = offering.Resources.FirstOrDefault(resource => resource.Id == notification.ResourceId) as MicrosoftTeamResource;

        if (resource is null)
        {
            _logger
                .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), ResourceErrors.NotFound(notification.ResourceId))
                .Error("Failed to complete the event handler");

            return;
        }

        List<StaffId> staffIds = offering.Teachers.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.StaffId).ToList();

        List<StaffMember> staffMembers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

        foreach (StaffMember staffMember in staffMembers)
        {
            TeacherAssignmentMSTeamOperation operation = new()
            {
                TeamName = resource.TeamName,
                StaffId = staffMember.Id,
                Action = MSTeamOperationAction.Add,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = _dateTime.Now
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}