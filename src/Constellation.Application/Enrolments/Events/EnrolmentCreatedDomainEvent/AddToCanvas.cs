﻿namespace Constellation.Application.Enrolments.Events.EnrolmentCreatedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Enrolments.Events;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddToCanvas
    : IDomainEventHandler<EnrolmentCreatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICanvasOperationsRepository _operationRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddToCanvas(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICanvasOperationsRepository operationRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _operationRepository = operationRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<EnrolmentCreatedDomainEvent>();
    }

    public async Task Handle(EnrolmentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Student.NotFound(notification.StudentId))
                .Error("Failed to complete the event handler");

            return;
        }

        Offering offering = await _offeringRepository.GetById(notification.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(EnrolmentDeletedDomainEvent), notification, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(notification.OfferingId))
                .Error("Failed to complete the event handler");

            return;
        }

        if (!offering.IsCurrent && offering.EndDate < _dateTime.Today)
            return;

        ModifyEnrolmentCanvasOperation operation = new()
        {
            UserId = student.StudentId,
            CourseId = $"{offering.EndDate.Year}-{offering.Name.Substring(0, offering.Name.Length - 1)}",
            Action = CanvasOperation.EnrolmentAction.Add,
            ScheduledFor = offering.IsCurrent ? _dateTime.Now : offering.StartDate.ToDateTime(TimeOnly.MinValue)
        };

        _operationRepository.Insert(operation);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}