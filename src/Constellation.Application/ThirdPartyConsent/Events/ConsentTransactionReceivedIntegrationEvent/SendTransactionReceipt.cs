#nullable enable
namespace Constellation.Application.ThirdPartyConsent.Events.ConsentTransactionReceivedIntegrationEvent;

using Abstractions;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.IntegrationEvents;
using Core.Models.ThirdPartyConsent;
using Core.Models.ThirdPartyConsent.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Services;
using Serilog;
using System;
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

    //TODO: R1.16.1: Send email to parent with responses in the transaction
    public async Task Handle(ConsentTransactionReceivedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        Transaction? transaction = await _consentRepository.GetTransactionById(notification.TransactionId, cancellationToken);

        if (transaction is null)
        {
            // Log error
            return;
        }

        Attachment document = await _attachmentService.GenerateConsentTransactionReceipt(transaction, cancellationToken);

        Result<EmailRecipient> parentRecipient = EmailRecipient.Create(transaction.ProvidedBy, transaction.ProvidedByEmailAddress.Email);

        if (parentRecipient.IsFailure)
        {
            // Log error
            return;
        }

        await _emailService.SendConsentTransactionReceiptToParent(
            [parentRecipient.Value],
            transaction.Student,
            DateOnly.FromDateTime(transaction.ProvidedAt),
            document,
            cancellationToken);

        // Generate and send email with attachment to:
        //  1. Parent listed on transaction
        //  2. Front office for records?
        //  3. Classroom teachers/Head teachers/Instructional Leader if any consents are denied
    }
}
 