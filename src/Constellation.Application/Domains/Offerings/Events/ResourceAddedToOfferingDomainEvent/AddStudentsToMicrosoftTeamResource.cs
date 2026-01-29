namespace Constellation.Application.Domains.Offerings.Events.ResourceAddedToOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.Enrolments.Repositories;
using Core.Models.LinkedSystems;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddStudentsToMicrosoftTeamResource
    : IDomainEventHandler<ResourceAddedToOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddStudentsToMicrosoftTeamResource(
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        ITeamOperationRepository operationsRepository,
        ITeamRepository teamRepository,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _studentRepository = studentRepository;
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

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Student? student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId))
                    .Error("Failed to add Student to Team");

                continue;
            }

            if (student.EmailAddress == EmailAddress.None)
            {
                _logger
                    .ForContext(nameof(ResourceAddedToOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), DomainErrors.ValueObjects.EmailAddress.EmailEmpty)
                    .Error("Failed to add Student to Team");

                continue;
            }

            ModifyTeamMembershipTeamOperation operation = new(
                teams.First().Id,
                student.EmailAddress,
                TeamAction.AddMember);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
