namespace Constellation.Application.Domains.EmergencyConsole.Commands.SendEmergencyMessageAsEmail;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Enums;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Threading.Tasks;

internal sealed class SendEmergencyMessageAsEmailCommandHandler
: ICommandHandler<SendEmergencyMessageAsEmailCommand>
{
    private readonly IEmailService _emailService;
    private readonly ISentMessageRepository _sentMessageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SendEmergencyMessageAsEmailCommandHandler(
        IEmailService emailService,
        ISentMessageRepository sentMessageRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _emailService = emailService;
        _sentMessageRepository = sentMessageRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SendEmergencyMessageAsEmailCommand request, CancellationToken cancellationToken)
    {
        List<EmailRecipient> recipients = [];

        // Calculate RecipientGroup recipients
        // Calculate Recipient recipients
        // Dedupe recipient list
        
        EventId eventId = new();

        foreach (EmailRecipient recipient in recipients)
        {
            Result<string> email = await _emailService.SendEmergencyConsoleEmail(recipient, request.Message, cancellationToken);

            Result<SentMessage> sentMessage = SentMessage.Create(eventId, MessageType.Email, recipient.Email, recipient.Name, request.Message);

            if (sentMessage.IsFailure)
            {
                _logger
                    .ForContext(nameof(SendEmergencyMessageAsEmailCommand), request, true)
                    .ForContext("Email Sent", email.IsSuccess)
                    .ForContext(nameof(Error), sentMessage.Error, true)
                    .ForContext(nameof(EmailRecipient), recipient, true)
                    .Warning("Failed to record Emergency message status");
                
                continue;
            }

            _sentMessageRepository.Insert(sentMessage.Value);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
    }
}
