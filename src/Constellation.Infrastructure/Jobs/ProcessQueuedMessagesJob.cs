namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Application.Interfaces.Services;
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
            .Where(m => m.ProcessedAt == null && m.Error == null)
            .OrderByDescending(m => m.Priority) // higher enum value = higher priority
            .ThenBy(m => m.QueuedAt) // within same priority, oldest first
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (QueuedMessage message in messages)
        {
            try
            {
                await SendAsync(message, cancellationToken);
                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(QueuedMessage), message, true)
                    .Error(ex, "Failed to process queued message {MessageId}. Marking as failed.", message.Id);

                message.MarkFailed(ex.ToString());
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SendAsync(
        QueuedMessage message, 
        CancellationToken cancellationToken = default)
    {
        int missedCount = 0;

        foreach (var recipient in message.Recipients)
        {
            bool sent = false;

            if (message.Type == MessageType.SMS && recipient.HasPhone)
            {
                Result<SmsRecipient> smsRecipient = SmsRecipient.Create(recipient.Name, recipient.PhoneNumber.ToString());

                if (smsRecipient.IsSuccess)
                {
                    await _smsService.SendQueuedMessage(message.Sender!, smsRecipient.Value, message.Body, cancellationToken);
                    sent = true;
                }
            }

            if (!sent && recipient.HasEmail)
            {
                Result<EmailRecipient> emailRecipient = EmailRecipient.Create(recipient.Name, recipient.EmailAddress);

                if (emailRecipient.IsSuccess)
                {
                    await _emailService.SendQueuedMessage(message.Sender!, emailRecipient.Value, message.Subject ?? string.Empty, message.Body, cancellationToken);
                    sent = true;
                }
            }

            if (!sent)
            {
                missedCount++;

                _logger
                    .Warning("Failed to send message {MessageId} to recipient {RecipientName}. No valid contact information.", message.Id, recipient.Name);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (missedCount == message.Recipients.Count || missedCount / message.Recipients.Count >= 0.25f)
        {
            throw new InvalidOperationException($"Message {message.Id} has no valid recipients.");
        }
    }
}