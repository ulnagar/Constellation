namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
using Application.Models.Identity;
using Core.Models.Messaging.Drafts;
using Core.Models.Messaging.Enums;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;

internal sealed class ProcessQueuedMessagesJob : IProcessQueuedMessagesJob
{
    private readonly AppDbContext _context;
    private readonly ISMSService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public ProcessQueuedMessagesJob(
        AppDbContext context,
        ISMSService smsService,
        IEmailService emailService,
        ILogger logger)
    {
        _context = context;
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger
            .ForContext<IProcessQueuedMessagesJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        List<QueuedMessage> messages = await _context.Set<QueuedMessage>()
            .Where(m => m.ProcessedAt == null && !m.HasErrors)
            .OrderByDescending(m => m.Priority) // higher enum value = higher priority
            .ThenBy(m => m.QueuedAt) // within same priority, oldest first
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (QueuedMessage message in messages)
        {
            try
            {
                await SendMessage(message, cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(QueuedMessage), message, true)
                    .Error(ex, "Failed to process queued message {MessageId}. Marking as failed.", message.Id);

                message.AddError(ExceptionError.FromException(ex));
            }

            if (message.Errors.Count > 0)
                await SendLog(message, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SendMessage(
        QueuedMessage message, 
        CancellationToken cancellationToken = default)
    {
        foreach (var recipient in message.Recipients)
        {
            bool sent = false;

            if (message.Type == MessageType.SMS && recipient.HasPhone)
            {
                Result<SmsRecipient> smsRecipient = SmsRecipient.Create(recipient.Name, recipient.PhoneNumber.ToString());

                if (smsRecipient.IsSuccess)
                {
                    Result result = await _smsService.SendQueuedMessage(message.Sender!, smsRecipient.Value, message.Body, cancellationToken);

                    if (result.IsSuccess)
                        sent = true;
                    else
                        message.AddError(new RecipientError(recipient, result.Error.ToString()));
                }
            }

            if (!sent && recipient.HasEmail)
            {
                Result<EmailRecipient> emailRecipient = EmailRecipient.Create(recipient.Name, recipient.EmailAddress.Email);

                if (emailRecipient.IsSuccess)
                {
                    Result result = await _emailService.SendQueuedMessage(message.Sender!, emailRecipient.Value, message.Subject ?? string.Empty, message.Body, cancellationToken);
                    
                    if (result.IsSuccess)
                        sent = true;
                    else
                        message.AddError(new RecipientError(recipient, result.Error.ToString()));
                }
            }

            if (!sent)
            {
                message.AddError(new RecipientError(recipient, "No valid contact information found."));

                _logger
                    .Warning("Failed to send message {MessageId} to recipient {RecipientName}. No valid contact information.", message.Id, recipient.Name);
            }
        }
    }

    private async Task SendLog(
        QueuedMessage message, 
        CancellationToken cancellationToken = default)
    {
        AppUser? user = await _context
            .Set<AppUser>()
            .FirstOrDefaultAsync(user => user.Id == message.UserId, cancellationToken);

        if (user is null)
            return;

        Result<EmailRecipient> logRecipient = EmailRecipient.Create(user.Name, user.Email ?? string.Empty);

        if (logRecipient.IsFailure)
            return;

        await _emailService.SendQueuedMessageLog(logRecipient.Value, message, cancellationToken);
    }
}