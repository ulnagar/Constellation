namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Commands.SendEmergencyMessage;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Enums;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Models.Messaging.EmergencyConsole.Services;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;

internal sealed class SendEmergencyMessageCommandHandler
: ICommandHandler<SendEmergencyMessageCommand>
{
    private readonly IMessageEventRepository _messageEventRepository;
    private readonly IEmergencyRecipientService _recipientService;
    private readonly IHangfireJobService _hangfireService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendEmergencyMessageCommandHandler(
        IMessageEventRepository messageEventRepository,
        IEmergencyRecipientService recipientService,
        IHangfireJobService hangfireService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _messageEventRepository = messageEventRepository;
        _recipientService = recipientService;
        _hangfireService = hangfireService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    public async Task<Result> Handle(SendEmergencyMessageCommand request, CancellationToken cancellationToken)
    {
        List<AlertRecipient> recipients = request.Recipients;

        foreach (RecipientGroup group in request.RecipientGroups)
        {
            List<AlertRecipient> groupRecipients = await _recipientService.GetSelectedRecipientsFromGroup(group, cancellationToken);

            recipients.AddRange(groupRecipients);
        }

        recipients = recipients.Distinct().ToList();

        Result<MessageEvent> messageEvent = MessageEvent.Create(request.Message, _dateTime.Now, _currentUserService.UserName);

        if (messageEvent.IsFailure)
        {
            _logger
                .ForContext(nameof(SendEmergencyMessageCommand), request, true)
                .ForContext(nameof(Error), messageEvent.Error, true)
                .Warning("Failed to record Emergency message status");

            return Result.Failure(messageEvent.Error);
        }

        _messageEventRepository.Insert(messageEvent.Value);

        foreach (AlertRecipient recipient in recipients)
        {
            MessageId messageId = messageEvent.Value.AddRecipient(request.Type, recipient);

            QueuedMessage item = new QueuedMessage(messageEvent.Value.Id, messageId, recipient);

            _messageEventRepository.Insert(item);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        await _hangfireService.EnqueueEmergencyMessageJob(messageEvent.Value.Id, cancellationToken);

        return Result.Success();
    }
}
