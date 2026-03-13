namespace Constellation.Core.Models.Messaging.Email;

using Constellation.Core.Shared;
using Enums;
using Errors;
using Identifiers;
using System;
using ValueObjects;

public sealed class EmailMessage
{
    private readonly List<EmailMessageRecipient> _recipients = [];
    private readonly List<EmailTrackingEvent> _events = [];

    public EmailId Id { get; init; } = new();
    public required string SendingModule { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? StatusUpdatedAt { get; set; }

    // Sender — single value, owned directly on the message
    public required EmailRecipient From { get; set; }
    public EmailRecipient? ReplyTo { get; set; }

    // Content
    public required string Subject { get; set; }
    public required string BodyText { get; set; }
    public required string BodyHtml { get; set; }

    // Status & Delivery
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public string? Provider { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }

    // Open tracking (denormalised for quick reads)
    public int OpenCount { get; set; } = 0;
    public DateTimeOffset? FirstOpenedAt { get; set; }
    public DateTimeOffset? LastOpenedAt { get; set; }

    // Metadata
    public string? TemplateId { get; set; }
    public string? Tags { get; set; }
    public string? Metadata { get; set; }

    // Navigation
    public IReadOnlyList<EmailMessageRecipient> Recipients => _recipients.AsReadOnly();
    public IReadOnlyList<EmailTrackingEvent> TrackingEvents => _events.AsReadOnly();

    public Result MarkSent(string? providerMessageId = null)
    {
        if (Status != EmailStatus.Pending)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, EmailStatus.Sent));

        Status = EmailStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        ProviderMessageId = providerMessageId;

        return Result.Success();
    }

    public Result MarkFailed(string errorMessage)
    {
        if (Status != EmailStatus.Pending)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, EmailStatus.Failed));

        Status = EmailStatus.Failed;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;

        return Result.Success();
    }

    public Result MarkDelivered()
    {
        if (Status != EmailStatus.Sent)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, EmailStatus.Delivered));

        Status = EmailStatus.Delivered;
        StatusUpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result MarkBounced(string? errorMessage = null)
    {
        if (Status != EmailStatus.Sent)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, EmailStatus.Bounced));

        Status = EmailStatus.Bounced;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;

        return Result.Success();
    }

    public Result AddRecipient(EmailRecipient recipient, EmailRecipientType recipientType)
    {
        if (_recipients.Any(r => r.Recipient.Email.Equals(recipient.Email, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(EmailMessagingErrors.DuplicateRecipient(recipient.Email));

        _recipients.Add(new EmailMessageRecipient
        {
            Recipient = recipient,
            Email = recipient.Email,
            RecipientType = recipientType,
            EmailId = Id
        });

        return Result.Success();
    }

    public Result AddRecipients(IEnumerable<EmailRecipient> recipients, EmailRecipientType recipientType)
    {
        // Validate the entire batch before adding any, to avoid partial updates
        List<EmailRecipient> incoming = recipients.ToList();

        List<string> duplicateIncoming = incoming
            .GroupBy(r => r.Email, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIncoming.Count != 0)
            return Result.Failure(EmailMessagingErrors.DuplicateRecipientInBatch(duplicateIncoming));

        List<string> duplicateExisting = incoming
            .Where(r => _recipients.Any(existing =>
                existing.Recipient.Email.Equals(r.Email, StringComparison.OrdinalIgnoreCase)))
            .Select(r => r.Email)
            .ToList();

        if (duplicateExisting.Count != 0)
            return Result.Failure(EmailMessagingErrors.DuplicateRecipient(duplicateExisting));

        foreach (EmailRecipient recipient in incoming)
        {
            _recipients.Add(new EmailMessageRecipient
            {
                Recipient = recipient,
                Email = recipient.Email,
                RecipientType = recipientType,
                EmailId = Id
            });
        }

        return Result.Success();
    }
}
