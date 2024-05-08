﻿namespace Constellation.Application.SchoolContacts.Events.SchoolContactRoleCreated;

using Abstractions.Messaging;
using Interfaces.Services;
using Constellation.Core.Models.SchoolContacts;
using Core.Abstractions.Clock;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Events;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

internal sealed class SendWelcomeEmail 
    : IDomainEventHandler<SchoolContactRoleCreatedDomainEvent>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEmailService _emailService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SendWelcomeEmail(
        ISchoolContactRepository contactRepository,
        IEmailService emailService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _emailService = emailService;
        _dateTime = dateTime;
        _logger = logger.ForContext<SchoolContactRoleCreatedDomainEvent>();
    }

    public async Task Handle(SchoolContactRoleCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        SchoolContact contact = await _contactRepository.GetById(notification.ContactId, cancellationToken);

        if (contact is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(notification.ContactId), true)
                .Warning("Failed to send welcome email to new School Contact");

            return;
        }

        SchoolContactRole role = contact.Assignments.FirstOrDefault(role => role.Id == notification.RoleId);

        if (role is null)
        {
            _logger
                .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                .ForContext(nameof(SchoolContact), contact, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFound(notification.RoleId), true)
                .Warning("Failed to send welcome email to new School Contact");

            return;
        }

        if (_dateTime.Now.Subtract(contact.CreatedAt).TotalDays < 5)
            return;

        switch (role.Role)
        {
            case SchoolContactRole.Coordinator when 
                contact.Assignments
                    .Count(assignment =>
                        !assignment.IsDeleted && 
                        assignment.Role == SchoolContactRole.Coordinator) == 1:
                {
                    // This is a new ACC without another Coordinator role.
                    List<EmailRecipient> recipients = new();

                    Result<EmailRecipient> recipient = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                    if (recipient.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                            .ForContext(nameof(SchoolContact), contact, true)
                            .ForContext(nameof(Error), recipient.Error, true)
                            .Warning("Failed to send welcome email to new School Contact");

                        return;
                    }

                    recipients.Add(recipient.Value);

                    await _emailService.SendWelcomeEmailToCoordinator(recipients, role.SchoolName, cancellationToken);
                    break;
                }
            case SchoolContactRole.SciencePrac when
                contact.Assignments
                    .Count(assignment =>
                        !assignment.IsDeleted &&
                        assignment.Role == SchoolContactRole.SciencePrac) == 1:
                {
                    // This is a new SPT without another SPT role.
                    List<EmailRecipient> recipients = new();

                    Result<EmailRecipient> recipient = EmailRecipient.Create(contact.DisplayName, contact.EmailAddress);

                    if (recipient.IsFailure)
                    {
                        _logger
                            .ForContext(nameof(SchoolContactRoleCreatedDomainEvent), notification, true)
                            .ForContext(nameof(SchoolContact), contact, true)
                            .ForContext(nameof(Error), recipient.Error, true)
                            .Warning("Failed to send welcome email to new School Contact");

                        return;
                    }

                    recipients.Add(recipient.Value);

                    await _emailService.SendWelcomeEmailToSciencePracTeacher(recipients, role.SchoolName, cancellationToken);
                    break;
                }
        }
    }
}