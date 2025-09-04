namespace Constellation.Core.Models.Awards;

using Enums;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueObjects;

public sealed class NominationNotification
{
    private readonly List<AwardNominationId> _nominations = [];
    private readonly List<EmailRecipient> _toAddresses = [];
    private readonly List<EmailRecipient> _ccAddresses = [];

    private NominationNotification() { }

    public NominationNotificationId Id { get; private set; }
    public AwardNominationPeriodId PeriodId { get; private set; }
    public AwardNotificationType Type { get; private set; }
    public IReadOnlyList<AwardNominationId> Nominations => _nominations.AsReadOnly();
    public DateTime SentAt { get; private set; }
    public EmailRecipient FromAddress { get; private set; }
    public IReadOnlyList<EmailRecipient> ToAddresses => _toAddresses.AsReadOnly();
    public IReadOnlyList<EmailRecipient> CcAddresses => _ccAddresses.AsReadOnly();
    public string Subject { get; private set; }
    public string Body { get; private set; }

    private static NominationNotification Create(
        AwardNominationPeriodId periodId,
        AwardNotificationType type,
        List<AwardNominationId> nominations,
        DateTime sentAt,
        EmailRecipient from,
        List<EmailRecipient> to,
        List<EmailRecipient> cc,
        string subject,
        string body)
    {
        NominationNotification notification = new() { 
            Id = new(),
            PeriodId = periodId, 
            
            Type = type, 
            SentAt = sentAt,
            FromAddress = from,
            Subject = subject,
            Body = body
        };

        foreach (EmailRecipient recipient in to)
        {
            if (notification._toAddresses.All(entry => entry.Email != recipient.Email))
                notification._toAddresses.Add(recipient);
        }

        foreach (EmailRecipient recipient in cc)
        {
            if (notification._ccAddresses.All(entry => entry.Email != recipient.Email))
                notification._ccAddresses.Add(recipient);
        }

        notification._nominations.AddRange(nominations);

        return notification;
    }
}