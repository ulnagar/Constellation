namespace Constellation.Application.Domains.Enrolments.Events.EnrolmentCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Enrolments.Errors;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.LinkedSystems.Errors;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Events;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Core.Models.Enrolments;
using Core.Models.LinkedSystems;
using Core.Models.Offerings.Enums;
using Core.Models.Offerings.ValueObjects;
using Core.Models.Operations;
using Core.Models.Operations.Enums;
using Core.Models.Operations.Repositories;
using Core.Models.Students.Errors;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToMicorosftTeams
    : IDomainEventHandler<EnrolmentCreatedDomainEvent>
{
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ITeamOperationRepository _operationRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToMicorosftTeams(
        IEnrolmentRepository enrolmentRepository,
        ITutorialRepository tutorialRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ITeamOperationRepository operationRepository,
        ITeamRepository teamRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _enrolmentRepository = enrolmentRepository;
        _tutorialRepository = tutorialRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _operationRepository = operationRepository;
        _teamRepository = teamRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentCreatedDomainEvent>();
    }

    public async Task Handle(EnrolmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Enrolment? enrolment = await _enrolmentRepository.GetById(notification.EnrolmentId, cancellationToken);

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentErrors.NotFound(notification.EnrolmentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        Student? student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        switch (enrolment)
        {
            case OfferingEnrolment offeringEnrolment:
                {
                    Offering? offering = await _offeringRepository.GetById(offeringEnrolment.OfferingId, cancellationToken);

                    if (offering is null)
                    {
                        _logger
                            .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                            .ForContext(nameof(Error), OfferingErrors.NotFound(offeringEnrolment.OfferingId))
                            .Error("Failed to complete the event handler");

                        return;
                    }

                    if (!offering.IsCurrent && offering.EndDate < _dateTime.Today)
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
                            student.EmailAddress,
                            TeamAction.AddMember);

                        if (!offering.IsCurrent)
                        {
                            operation.UpdateSchedule(offering.StartDate.ToDateTime(TimeOnly.MinValue));
                        }

                        _operationRepository.Insert(operation);
                    }

                    break;
                }
            case TutorialEnrolment tutorialEnrolment:
                {
                    Tutorial? tutorial = await _tutorialRepository.GetById(tutorialEnrolment.TutorialId, cancellationToken);

                    if (tutorial is null)
                    {
                        _logger
                            .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                            .ForContext(nameof(Error), TutorialErrors.NotFound(tutorialEnrolment.TutorialId))
                            .Error("Failed to complete the event handler");

                        return;
                    }

                    if (!tutorial.IsCurrent && tutorial.EndDate < _dateTime.Today)
                        return;

                    foreach (TeamsResource resource in tutorial.Teams)
                    {
                        ModifyTeamMembershipTeamOperation operation = new(
                            resource.TeamId,
                            student.EmailAddress,
                            TeamAction.AddMember);

                        if (!tutorial.IsCurrent)
                        {
                            operation.UpdateSchedule(tutorial.StartDate.ToDateTime(TimeOnly.MinValue));
                        }

                        _operationRepository.Insert(operation);
                    }

                    break;
                }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}