namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNotification;

using Core.Models.Awards.Enums;
using Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record NotificationResponse(
    AwardNotificationType Type,
    DateTime SentAt,
    EmailRecipient FromAddress,
    List<EmailRecipient> ToAddresses,
    List<EmailRecipient> CcAddresses,
    string Subject,
    string Body);