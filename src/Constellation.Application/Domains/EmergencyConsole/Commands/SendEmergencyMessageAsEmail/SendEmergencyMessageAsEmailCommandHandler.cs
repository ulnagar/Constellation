namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsEmail;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Repositories;
using Core.Models.EmergencyConsole.Services;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Interfaces.Services;
using Org.BouncyCastle.Cms;
using Serilog;
using System.Threading.Tasks;

internal sealed class SendEmergencyMessageAsEmailCommandHandler
: ICommandHandler<SendEmergencyMessageAsEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly ISentMessageRepository _sentMessageRepository;
    private readonly IEmergencyRecipientService _recipientService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendEmergencyMessageAsEmailCommandHandler(
        IEmailService emailService,
        ISentMessageRepository sentMessageRepository,
        IEmergencyRecipientService recipientService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _emailService = emailService;
        _sentMessageRepository = sentMessageRepository;
        _recipientService = recipientService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmergencyMessageAsEmailCommand request, CancellationToken cancellationToken)
    {
        List<AlertRecipient> recipients = request.Recipients;

        foreach (RecipientGroup group in request.RecipientGroups)
        {
            List<AlertRecipient> groupRecipients = await _recipientService.GetSelectedRecipientsFromGroup(group, cancellationToken);

            recipients.AddRange(groupRecipients);
        }

        recipients = recipients.Distinct().ToList();
        
        Result<SentMessage> sentMessage = SentMessage.Create(request.Message);

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
            if (recipient.HasEmail)
            {
                Result<string> email = await _emailService.SendEmergencyConsoleEmail(recipient, request.Message, cancellationToken);

                sentMessage.Value.AddMessage(MessageType.Email, recipient.EmailAddress.Email, recipient.Name, email.IsSuccess);
            }
            else
            {
                sentMessage.Value.AddMessage(MessageType.Email, string.Empty, recipient.Name, false);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
