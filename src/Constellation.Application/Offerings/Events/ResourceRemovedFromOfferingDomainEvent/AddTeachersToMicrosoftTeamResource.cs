namespace Constellation.Application.Offerings.Events.ResourceRemovedFromOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
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
    private readonly IMSTeamOperationsRepository _operationsRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveTeachersFromMicrosoftTeamResource(
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
        _logger = logger.ForContext<ResourceRemovedFromOfferingDomainEvent>();
    }

    public async Task Handle(ResourceRemovedFromOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Resource.Type != ResourceType.MicrosoftTeam)
            return;

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        MicrosoftTeamResource resource = notification.Resource as MicrosoftTeamResource;

        List<string> staffIds = offering.Teachers.Where(assignment => !assignment.IsDeleted).Select(assignment => assignment.StaffId).ToList();

        List<Staff> staffMembers = await _staffRepository.GetListFromIds(staffIds, cancellationToken);

        foreach (Staff staffMember in staffMembers)
        {
            TeacherAssignmentMSTeamOperation operation = new()
            {
                TeamName = resource.TeamName,
                StaffId = staffMember.StaffId,
                Action = MSTeamOperationAction.Remove,
                PermissionLevel = MSTeamOperationPermissionLevel.Owner,
                DateScheduled = _dateTime.Now
            };

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}