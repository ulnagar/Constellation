namespace Constellation.Core.Models.Messaging.Email;

using Constellation.Core.Shared;
using Enums;
using Errors;
using Identifiers;
using Messaging.Enums;
using Primitives;
using System;
using ValueObjects;

public sealed class EmailMessage : IHasCreatedAt
{
    private readonly List<EmailMessageRecipient> _recipients = [];
    private readonly List<EmailTrackingEvent> _events = [];
    private readonly List<EmailLink> _links = [];

    public EmailId Id { get; init; } = new();
    public required string SendingModule { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? StatusUpdatedAt { get; set; }

    // Sender — single value, owned directly on the message
    public required MessageSender From { get; set; }
    public MessageSender? ReplyTo { get; set; }

    // Content
    public required string Subject { get; set; }
    public required string BodyText { get; set; }
    public required string BodyHtml { get; set; }

    // Status & Delivery
    public MessageStatus Status { get; set; } = MessageStatus.Pending;
    public string? Provider { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? ErrorMessage { get; set; }

    // Open tracking (denormalised for quick reads)
    public int OpenCount { get; set; } = 0;
    public DateTimeOffset? FirstOpenedAt { get; set; }
    public DateTimeOffset? LastOpenedAt { get; set; }
    public int ClickCount { get; set; } = 0;
    public DateTimeOffset? FirstClickedAt { get; set; }
    public DateTimeOffset? LastClickedAt { get; set; }


    // Metadata
    public string? TemplateId { get; set; }
    public string? Tags { get; set; }
    public string? Metadata { get; set; }

    // Navigation
    public IReadOnlyList<EmailMessageRecipient> Recipients => _recipients.AsReadOnly();
    public IReadOnlyList<EmailTrackingEvent> TrackingEvents => _events.AsReadOnly();
    public IReadOnlyList<EmailLink> Links => _links.AsReadOnly();

    public Result MarkSent(string? providerMessageId = null)
    {
        if (Status != MessageStatus.Pending)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, MessageStatus.Sent));

        Status = MessageStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        ProviderMessageId = providerMessageId;

        return Result.Success();
    }

    public Result MarkFailed(string errorMessage)
    {
        if (Status != MessageStatus.Pending)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, MessageStatus.Error));

        Status = MessageStatus.Error;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        ErrorMessage = errorMessage;

        return Result.Success();
    }

    public Result MarkDelivered()
    {
        if (Status != MessageStatus.Sent)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, MessageStatus.Delivered));

        Status = MessageStatus.Delivered;
        StatusUpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result AddRecipient(EmailRecipient recipient, EmailRecipientType recipientType)
    {
        if (_recipients.Any(r => r.Email.Equals(recipient.Email, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(EmailMessagingErrors.DuplicateRecipient(recipient.Email));

        _recipients.Add(new EmailMessageRecipient
        {
            Name = recipient.Name,
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
                existing.Email.Equals(r.Email, StringComparison.OrdinalIgnoreCase)))
            .Select(r => r.Email)
            .ToList();

        if (duplicateExisting.Count != 0)
            return Result.Failure(EmailMessagingErrors.DuplicateRecipient(duplicateExisting));

        foreach (EmailRecipient recipient in incoming)
        {
            _recipients.Add(new EmailMessageRecipient
            {
                Name = recipient.Name,
                Email = recipient.Email,
                RecipientType = recipientType,
                EmailId = Id
            });
        }

        return Result.Success();
    }

    public Result RegisterLink(string destinationUrl)
    {
        if (_links.Any(l => l.DestinationUrl.Equals(destinationUrl, StringComparison.OrdinalIgnoreCase)))
            return Result.Success(); // Already registered — not an error, just a duplicate link in the template

        _links.Add(new EmailLink
        {
            DestinationUrl = destinationUrl,
            EmailId = Id
        });

        return Result.Success();
    }
}
