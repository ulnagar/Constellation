namespace Constellation.Application.Domains.Enrolments.Events.EnrolmentDeletedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments.Errors;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Enrolments.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Repositories;
using Constellation.Core.Shared;
using Core.Models.Enrolments;
using Core.Models.Students.Errors;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveFromMicrosoftTeams
    : IDomainEventHandler<EnrolmentDeletedDomainEvent>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IMSTeamOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public RemoveFromMicrosoftTeams(
        ITutorialRepository tutorialRepository,
        IEnrolmentRepository enrolmentRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        IMSTeamOperationsRepository operationRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _enrolmentRepository = enrolmentRepository;
        _unitOfWork = unitOfWork;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _operationRepository = operationRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<EnrolmentDeletedDomainEvent>();
    }

    public async Task Handle(EnrolmentDeletedDomainEvent notification, CancellationToken cancellationToken)
    {
        Enrolment enrolment = await _enrolmentRepository.GetById(notification.EnrolmentId, cancellationToken);

        if (enrolment is null)
        {
            _logger
                .ForContext(nameof(EnrolmentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), EnrolmentErrors.NotFound(notification.EnrolmentId), true)
                .Error("Failed to complete the event handler");

            return;
        }

        Student student = await _studentRepository.GetById(enrolment.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(enrolment.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        switch (enrolment)
        {
            case OfferingEnrolment offeringEnrolment:
                {
                    Offering offering = await _offeringRepository.GetById(offeringEnrolment.OfferingId, cancellationToken);

                    if (offering is null)
                    {
                        _logger
                            .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                            .ForContext(nameof(Error), OfferingErrors.NotFound(offeringEnrolment.OfferingId))
                            .Error("Failed to complete the event handler");

                        return;
                    }

                    if (!offering.IsCurrent && offering.EndDate < _dateTime.Today)
                        return;

                    StudentMSTeamOperation operation = new()
                    {
                        StudentId = student.Id,
                        OfferingId = offering.Id,
                        DateScheduled = _dateTime.Now,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Member
                    };

                    _operationRepository.Insert(operation);
                    break;
                }
            case TutorialEnrolment tutorialEnrolment:
                {
                    Tutorial tutorial = await _tutorialRepository.GetById(tutorialEnrolment.TutorialId, cancellationToken);

                    if (tutorial is null)
                    {
                        _logger
                            .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                            .ForContext(nameof(Error), TutorialErrors.NotFound(tutorialEnrolment.TutorialId))
                            .Error("Failed to complete the event handler");

                        return;
                    }

                    if (!tutorial.IsCurrent && tutorial.EndDate < _dateTime.Today)
                        return;

                    StudentTutorialMSTeamOperation operation = new()
                    {
                        StudentId = student.Id,
                        TutorialId = tutorial.Id,
                        DateScheduled = _dateTime.Now,
                        Action = MSTeamOperationAction.Remove,
                        PermissionLevel = MSTeamOperationPermissionLevel.Member
                    };

                    _operationRepository.Insert(operation);
                    break;
                }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
