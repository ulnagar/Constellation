namespace Constellation.Application.Domains.Messaging.Sms.Events.SmsMessageReceived;

using Abstractions.Messaging;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Errors;
using Core.Models.Messaging.Sms.Events;
using Core.Models.Messaging.Sms.Repositories;
using Core.Shared;
using Interfaces.Services;
using Serilog;

internal sealed class EmailSchoolAdmin
: IDomainEventHandler<SmsMessageReceivedDomainEvent>
{
    private readonly ISmsRepository _smsRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public EmailSchoolAdmin(
        ISmsRepository smsRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _smsRepository = smsRepository;
        _emailService = emailService;
        _logger = logger
            .ForContext<SmsMessageReceivedDomainEvent>();
    }

    public async Task Handle(SmsMessageReceivedDomainEvent notification, CancellationToken cancellationToken)
    {
        SmsMessage? message = await _smsRepository.GetById(notification.SmsId, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(SmsMessageReceivedDomainEvent), notification, true)
                .ForContext(nameof(Error), SmsMessagingErrors.NotFound(notification.SmsId), true)
                .Warning("Failed to forward received SMS");

            return;
        }

        await _emailService.SendIncomingSmsAlert(message, cancellationToken);
    }
}
