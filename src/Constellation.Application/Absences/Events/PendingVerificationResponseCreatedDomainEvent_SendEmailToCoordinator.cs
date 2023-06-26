﻿namespace Constellation.Application.Absences.Events;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.DomainEvents;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using MediatR;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator
    : INotificationHandler<PendingVerificationResponseCreatedDomainEvent>
{
    private readonly IAbsenceRepository _absenceRepository;
    private readonly IAbsenceResponseRepository _responseRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public PendingVerificationResponseCreatedDomainEvent_SendEmailToCoordinator(
        IAbsenceRepository absenceRepository,
        IAbsenceResponseRepository responseRepository,
        IStudentRepository studentRepository,
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _absenceRepository = absenceRepository;
        _responseRepository = responseRepository;
        _studentRepository = studentRepository;
        _contactRepository = contactRepository;
        _emailService = emailService;
        _logger = logger.ForContext<PendingVerificationResponseCreatedDomainEvent>();
    }

    public async Task Handle(PendingVerificationResponseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Send email to ACC to let them know there is a response that requires verification.
        var absence = await _absenceRepository.GetById(notification.AbsenceId, cancellationToken);

        if (absence is null)
        {
            _logger.Warning("Could not locate absence with Id {id}", notification.AbsenceId);
            return;
        }

        var response = await _responseRepository.GetById(notification.ResponseId, cancellationToken);

        if (response is null)
        {
            _logger.Warning("Could not locate absence response with Id {id}", notification.ResponseId);
            return;
        }

        var student = await _studentRepository.GetWithSchoolById(absence.StudentId, cancellationToken);

        if (student is null)
        {
            _logger.Warning("Could not locate student {studentId} related to absence with Id {id}", absence.StudentId, absence.Id);
            return;
        }

        List<SchoolContact> coordinators = await _contactRepository.GetBySchoolAndRole(student.SchoolCode, SchoolContactRole.Coordinator, cancellationToken);

        List<EmailRecipient> recipients = new();

        foreach (SchoolContact coordinator in coordinators)
        {
            Result<Name> nameResult = Name.Create(coordinator.FirstName, string.Empty, coordinator.LastName);
            if (nameResult.IsFailure)
                continue;

            Result<EmailRecipient> result = EmailRecipient.Create(nameResult.Value.DisplayName, coordinator.EmailAddress);

            if (result.IsSuccess && recipients.All(recipient => result.Value.Email != recipient.Email))
                recipients.Add(result.Value);
        }

        var message = await _emailService.SendCoordinatorPartialAbsenceVerificationRequest(new List<Absence> { absence }, student, recipients);

        if (message == null)
            return;

        string emails = string.Join(", ", recipients.Select(entry => entry.Email));

        absence.AddNotification(
            NotificationType.Email,
            message.message,
            emails);

        foreach (EmailRecipient recipient in recipients)
            _logger.Information("Partial Absence Verification Request send to {recipient} for absence by {student} on {date}", recipient.Name, student.DisplayName, absence.Date);
    }
}
