#nullable enable
namespace Constellation.Application.Domains.ThirdPartyConsent.Events.ConsentTransactionReceivedIntegrationEvent;

using AppSettings.Models;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.IntegrationEvents;
using Constellation.Core.Models.AppSettings.Enums;
using Core.Errors;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Extensions;
using Interfaces.Services;
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
    private readonly IAppSettingsService _appSettings;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public NotifyStaffOfRefusedConsent(
        IConsentRepository consentRepository,
        IAppSettingsService appSettings,
        IEmailService emailService,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _appSettings = appSettings;
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

        List<EmailRecipient> recipients = [];

        ContactsConfiguration? instructionalLeaders = await _appSettings.Contacts(ContactPosition.InstructionalLeader, cancellationToken);

        if (instructionalLeaders is null)
        {
            _logger
                .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(ContactsConfiguration)), true)
                .Warning("Failed to send notification of refused consent");
        }
        else
        {
            foreach (var instructionalLeader in instructionalLeaders.Contacts)
            {
                if (!instructionalLeader.Value.Contains(transaction.Grade))
                    continue;

                Result<EmailRecipient> recipient = instructionalLeader.Key.GetEmailRecipient;

                if (recipient.IsSuccess)
                    recipients.Add(recipient.Value);
            }
        }

        ContactsConfiguration? deputyPrincipals = await _appSettings.Contacts(ContactPosition.DeputyPrincipal, cancellationToken);

        if (deputyPrincipals is null)
        {
            _logger
                .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                .ForContext(nameof(Error), ApplicationErrors.InvalidConfiguration(nameof(ContactsConfiguration)), true)
                .Warning("Failed to send notification of refused consent");
        }
        else
        {
            foreach (var deputyPrincipal in deputyPrincipals.Contacts)
            {
                if (!deputyPrincipal.Value.Contains(transaction.Grade))
                    continue;

                Result<EmailRecipient> recipient = deputyPrincipal.Key.GetEmailRecipient;

                if (recipient.IsSuccess)
                    recipients.Add(recipient.Value);
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
 