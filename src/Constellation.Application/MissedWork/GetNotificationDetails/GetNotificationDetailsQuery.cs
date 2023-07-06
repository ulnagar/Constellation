namespace Constellation.Application.MissedWork.GetNotificationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetNotificationDetailsQuery(
    ClassworkNotificationId NotificationId)
    : IQuery<NotificationDetails>;