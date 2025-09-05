namespace Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNotification;

using Abstractions.Messaging;
using Core.Models.Awards.Identifiers;

public sealed record GetNotificationQuery(
    NominationNotificationId NotificationId)
    : IQuery<NotificationResponse>;