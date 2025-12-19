namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsSMS;

using Abstractions.Messaging;
using Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsEmail;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.EmergencyConsole;
using Constellation.Core.Models.EmergencyConsole.Enums;
using Constellation.Core.Models.EmergencyConsole.Repositories;
using Constellation.Core.Models.EmergencyConsole.Services;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Shared;
using Core.ValueObjects;
using Serilog;
using System.Threading.Tasks;

internal sealed class SendEmergencyMessageAsSMSCommandHandler
: ICommandHandler<SendEmergencyMessageAsSMSCommand>
{
    private readonly ISMSService _smsService;
    private readonly ISentMessageRepository _sentMessageRepository;
    private readonly IEmailService _emailService;
    private readonly IEmergencyRecipientService _recipientService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendEmergencyMessageAsSMSCommandHandler(
        ISMSService smsService,
        ISentMessageRepository sentMessageRepository,
        IEmailService emailService,
        IEmergencyRecipientService recipientService,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _smsService = smsService;
        _sentMessageRepository = sentMessageRepository;
        _emailService = emailService;
        _recipientService = recipientService;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<SendEmergencyMessageAsSMSCommand>();
    }

    public async Task<Result> Handle(SendEmergencyMessageAsSMSCommand request, CancellationToken cancellationToken)
    {
        List<AlertRecipient> recipients = request.Recipients;

        foreach (RecipientGroup group in request.RecipientGroups)
        {
            List<AlertRecipient> groupRecipients = await _recipientService.GetSelectedRecipientsFromGroup(group, cancellationToken);

            recipients.AddRange(groupRecipients);
        }

        recipients = recipients.Distinct().ToList();

        Result<SentMessage> sentMessage = SentMessage.Create(request.Message, _dateTime.Now, _currentUserService.UserName);

        if (sentMessage.IsFailure)
        {
            _logger
                .ForContext(nameof(SendEmergencyMessageAsEmailCommand), request, true)
                .ForContext(nameof(Error), sentMessage.Error, true)
                .Warning("Failed to record Emergency message status");

            return Result.Failure(sentMessage.Error);
        }

        _sentMessageRepository.Insert(sentMessage.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        foreach (AlertRecipient recipient in recipients)
        {
            if (recipient.HasPhone && recipient.PhoneNumber.IsMobile)
            {
                Result<string> email = await _smsService.SendEmergencyConsoleSms(recipient, request.Message, cancellationToken);

                sentMessage.Value.AddMessage(MessageType.SMS, recipient.PhoneNumber.ToString(PhoneNumber.Format.None), recipient.Name, email.IsSuccess);
            }
            else if (recipient.HasEmail)
            {
                Result<string> email = await _emailService.SendEmergencyConsoleEmail(recipient, request.Message, cancellationToken);

                sentMessage.Value.AddMessage(MessageType.Email, recipient.EmailAddress.Email, recipient.Name, email.IsSuccess);
            }
            else
            {
                sentMessage.Value.AddMessage(MessageType.SMS, string.Empty, recipient.Name, false);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
 