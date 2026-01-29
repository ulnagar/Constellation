namespace Constellation.Application.Domains.Offerings.Events.ResourceRemovedFromOfferingDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Enrolments;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Models.Enrolments.Repositories;
using Core.Models.LinkedSystems;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveStudentsFromMicrosoftTeamResource
    : IDomainEventHandler<ResourceRemovedFromOfferingDomainEvent>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ITeamOperationRepository _operationsRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveStudentsFromMicrosoftTeamResource(
        IOfferingRepository offeringRepository,
        IEnrolmentRepository enrolmentRepository,
        ITeamOperationRepository operationsRepository,
        ITeamRepository teamRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _enrolmentRepository = enrolmentRepository;
        _operationsRepository = operationsRepository;
        _teamRepository = teamRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ResourceRemovedFromOfferingDomainEvent>();
    }

    public async Task Handle(ResourceRemovedFromOfferingDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Resource is null)
            return;

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

        List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByOfferingId(offering.Id, cancellationToken);

        foreach (Enrolment enrolment in enrolments)
        {
            Student? student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

            if (student is null)
            {
                _logger
                    .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId))
                    .Error("Failed to add Student to Team");

                continue;
            }

            if (student.EmailAddress == EmailAddress.None)
            {
                _logger
                    .ForContext(nameof(ResourceRemovedFromOfferingDomainEvent), notification, true)
                    .ForContext(nameof(Error), DomainErrors.ValueObjects.EmailAddress.EmailEmpty)
                    .Error("Failed to add Student to Team");

                continue;
            }

            ModifyTeamMembershipTeamOperation operation = new(
                teams.First().Id,
                student.EmailAddress,
                TeamAction.Remove);

            _operationsRepository.Insert(operation);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
