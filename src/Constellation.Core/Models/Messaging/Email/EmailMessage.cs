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

    private EmailMessage() { }

    public EmailMessage(
        string sendingModule,
        MessageSender from,
        MessageSender? replyTo,
        string subject,
        string bodyText,
        string bodyHtml)
    {
        Id = new();

        SendingModule = sendingModule;
        CreatedAt = DateTimeOffset.UtcNow;

        From = from;
        ReplyTo = replyTo;

        Subject = subject;
        BodyText = bodyText;
        BodyHtml = bodyHtml;

        Status = MessageStatus.Pending;
    }

    public EmailId Id { get; init; }
    public string SendingModule { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? StatusUpdatedAt { get; private set; }

    // Sender — single value, owned directly on the message
    public MessageSender From { get; private set; }
    public MessageSender? ReplyTo { get; private set; }

    // Content
    public string Subject { get; private set; }
    public string BodyText { get; private set; }
    public string BodyHtml { get; private set; }

    // Status & Delivery
    public MessageStatus Status { get; private set; }
    public string? Provider { get; private set; } = string.Empty;
    public string? ProviderMessageId { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    // Open tracking (denormalised for quick reads)
    public int OpenCount { get; private set; }
    public DateTimeOffset? FirstOpenedAt { get; private set; }
    public DateTimeOffset? LastOpenedAt { get; private set; }
    public int ClickCount { get; private set; } 
    public DateTimeOffset? FirstClickedAt { get; private set; }
    public DateTimeOffset? LastClickedAt { get; private set; }


    // Metadata
    public string? TemplateId { get; private set; } = string.Empty;
    public string? Tags { get; private set; } = string.Empty;
    public string? Metadata { get; private set; } = string.Empty;

    // Navigation
    public IReadOnlyList<EmailMessageRecipient> Recipients => _recipients.AsReadOnly();
    public IReadOnlyList<EmailTrackingEvent> TrackingEvents => _events.AsReadOnly();
    public IReadOnlyList<EmailLink> Links => _links.AsReadOnly();

    public Result MarkSent(string? provider = null, string? providerMessageId = null)
    {
        if (Status != MessageStatus.Pending)
            return Result.Failure(EmailMessagingErrors.InvalidStatusTransition(Status, MessageStatus.Sent));

        Status = MessageStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        StatusUpdatedAt = DateTimeOffset.UtcNow;
        Provider = provider ?? string.Empty;
        ProviderMessageId = providerMessageId ?? string.Empty;

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

    public void RegisterLink(string destinationUrl)
    {
        if (_links.Any(l => l.DestinationUrl.Equals(destinationUrl, StringComparison.OrdinalIgnoreCase)))
            return; // Already registered — not an error, just a duplicate link in the template

        _links.Add(EmailLink.Create(Id, destinationUrl));
    }
}
