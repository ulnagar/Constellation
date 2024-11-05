#nullable enable
namespace Constellation.Application.ThirdPartyConsent.Events.ConsentTransactionReceivedIntegrationEvent;

using Abstractions;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.IntegrationEvents;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Errors;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

internal class SendTransactionReceipt
    : IIntegrationEventHandler<ConsentTransactionReceivedIntegrationEvent>
{
    private readonly IConsentRepository _consentRepository;
    private readonly IEmailAttachmentService _attachmentService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendTransactionReceipt(
        IConsentRepository consentRepository,
        IEmailAttachmentService attachmentService,
        IEmailService emailService,
        ILogger logger)
    {
        _consentRepository = consentRepository;
        _attachmentService = attachmentService;
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
                .Error("Failed to generate and send Consent Transaction receipt");

            return;
        }

        Attachment document = await _attachmentService.GenerateConsentTransactionReceipt(transaction, cancellationToken);

        Result<EmailRecipient> parentRecipient = EmailRecipient.Create(transaction.ProvidedBy, transaction.ProvidedByEmailAddress.Email);

        if (parentRecipient.IsFailure)
        {
            _logger
                .ForContext(nameof(ConsentTransactionReceivedIntegrationEvent), notification, true)
                .ForContext(nameof(Error), parentRecipient.Error, true)
                .Error("Failed to generate and send Consent Transaction receipt");

            return;
        }

        await _emailService.SendConsentTransactionReceiptToParent(
            [parentRecipient.Value],
            transaction.Student,
            DateOnly.FromDateTime(transaction.ProvidedAt),
            document,
            cancellationToken);
    }
}
 