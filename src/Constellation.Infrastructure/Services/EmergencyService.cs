namespace Constellation.Infrastructure.Services;

using Application.Extensions;
using Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.EmergencyConsole;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.EmergencyConsole.Errors;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Repositories;
using Core.Models.EmergencyConsole.Services;

internal sealed class EmergencyService : IEmergencyService
{
    private readonly IMessageEventRepository _eventRepository;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EmergencyService(
        IMessageEventRepository eventRepository,
        ISMSService smsService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _eventRepository = eventRepository;
        _smsService = smsService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<EmergencyService>();
    }
    public async Task SendEmergencyAlerts(EventId eventId, CancellationToken cancellationToken = default)
    {
        MessageEvent? messageEvent = await _eventRepository.GetEventById(eventId, cancellationToken);

        if (messageEvent is null)
        {
            _logger
                .ForContext(nameof(EventId), eventId, true)
                .ForContext(nameof(Error), MessageEventErrors.NotFound(eventId), true)
                .Warning("Failed to send Emergency message status");

            return;
        }

        List<QueuedMessage> queuedItems = await _eventRepository.GetQueuedMessagesByEventId(eventId, cancellationToken);

        foreach (QueuedMessage item in queuedItems)
        {
            MessageEventRecipient? matchingEntry = messageEvent.Recipients
                .FirstOrDefault(entry => entry.Id == item.MessageId);

            if (matchingEntry is null)
                continue;

            if (matchingEntry.Type == MessageType.SMS && item.AlertRecipient.HasPhone)
            {
                Result<string?> result = await _smsService.SendEmergencyConsoleSms(item.AlertRecipient, messageEvent.Message, cancellationToken);

                matchingEntry.UpdateRecipient(
                    MessageType.SMS,
                    item.AlertRecipient.PhoneNumber.ToString(PhoneNumber.Format.None),
                    result.IsFailure ? MessageStatus.Error : MessageStatus.Sent);
            }
            else if (item.AlertRecipient.HasEmail)
            {
                Result result = await _emailService.SendEmergencyConsoleEmail(item.AlertRecipient, messageEvent.Message.ToHtml(), cancellationToken);

                matchingEntry.UpdateRecipient(
                    MessageType.Email,
                    item.AlertRecipient.EmailAddress,
                    result.IsFailure ? MessageStatus.Error : MessageStatus.Sent);
            }
            else
            {
                matchingEntry.UpdateRecipient(
                    MessageType.Email,
                    EmailAddress.None,
                    MessageStatus.Error);
            }

            _eventRepository.Remove(item);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}