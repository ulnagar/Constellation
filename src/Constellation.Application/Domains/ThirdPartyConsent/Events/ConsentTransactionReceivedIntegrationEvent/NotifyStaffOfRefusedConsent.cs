﻿#nullable enable
namespace Constellation.Application.Domains.ThirdPartyConsent.Events.ConsentTransactionReceivedIntegrationEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.IntegrationEvents;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.StaffMembers.ValueObjects;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Configuration;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal class NotifyStaffOfRefusedConsent
    : IIntegrationEventHandler<ConsentTransactionReceivedIntegrationEvent>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly AppConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public NotifyStaffOfRefusedConsent(
        IConsentRepository consentRepository,
        IOptions<AppConfiguration> configuration,
        IStaffRepository staffRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _staffRepository = staffRepository;
        _configuration = configuration.Value;
        _emailService = emailService;
        _logger = logger
            .ForContext<ConsentTransactionReceivedIntegrationEvent>();
    }

    public async Task Handle(ConsentTransactionReceivedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Transaction? transaction = await _consentRepository.GetTransactionById(notification.TransactionId, cancellationToken);

        if (transaction is null)
        {
            _logger
                .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                .ForContext(nameof(Error), ConsentTransactionErrors.NotFound(notification.TransactionId), true)
                .Error("Failed to send notification of refused consent");

            return;
        }

        List<Transaction.ConsentResponse> responses = transaction.Responses
            .Where(entry => !entry.ConsentProvided)
            .ToList();

        if (responses.Count == 0)
            return;

        List<EmailRecipient> recipients = new();

        StaffMember? instructionalLeader = await _staffRepository.GetByEmployeeId(_configuration.Contacts.InstructionalLeader, cancellationToken);

        if (instructionalLeader is null)
        {
            _logger
                .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(_configuration.Contacts.InstructionalLeader), true)
                .Warning("Failed to send notification of refused consent");
        }
        else
        {
            Result<EmailRecipient> instructionalLeaderRecipient = EmailRecipient.Create(instructionalLeader.Name, instructionalLeader.EmailAddress);

            if (instructionalLeaderRecipient.IsFailure)
            {
                _logger
                    .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                    .ForContext(nameof(Error), instructionalLeaderRecipient.Error, true)
                    .Warning("Failed to send notification of refused consent");
            }
            else
            {
                recipients.Add(instructionalLeaderRecipient.Value);
            }
        }

        List<EmployeeId> deputyIds = _configuration.Contacts.DeputyPrincipalIds[transaction.Grade].ToList();

        foreach (EmployeeId staffId in deputyIds)
        {
            StaffMember? deputy = await _staffRepository.GetByEmployeeId(staffId, cancellationToken);

            if (deputy is null)
            {
                _logger
                    .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                    .ForContext(nameof(Error), StaffMemberErrors.NotFoundByEmployeeId(staffId), true)
                    .Warning("Failed to send notification of refused consent");
            }
            else
            {
                Result<EmailRecipient> deputyRecipient = EmailRecipient.Create(deputy.Name, deputy.EmailAddress);

                if (deputyRecipient.IsFailure)
                {
                    _logger
                        .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                        .ForContext(nameof(Error), deputyRecipient.Error, true)
                        .Warning("Failed to send notification of refused consent");
                }
                else
                {
                    recipients.Add(deputyRecipient.Value);
                }
            }
        }
        
        await _emailService.SendConsentRefusedNotification(
            recipients,
            transaction.Student,
            DateOnly.FromDateTime(transaction.ProvidedAt),
            responses,
            cancellationToken);
    }
}
 