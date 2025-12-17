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
        List<EmailRecipient> recipients = [];

        foreach (RecipientGroup group in request.RecipientGroups)
        {
            List<EmailRecipient> groupRecipients = await _recipientService.GetSelectedEmailRecipientsFromGroup(group, cancellationToken);

            recipients.AddRange(groupRecipients);
        }

        string[] manualRecipients = request.Recipients.Split(';');

        foreach (string manualRecipient in manualRecipients)
        {
            Result<EmailRecipient> recipient = EmailRecipient.Create(manualRecipient, manualRecipient);

            if (recipient.IsSuccess)
                recipients.Add(recipient.Value);
        }

        recipients = recipients.DistinctBy(entry => entry.Email).ToList();
        
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

        foreach (EmailRecipient recipient in recipients)
        {
            Result<string> email = await _emailService.SendEmergencyConsoleEmail(recipient, request.Message, cancellationToken);

            sentMessage.Value.AddMessage(MessageType.Email, recipient.Email, recipient.Name, email.IsSuccess);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
