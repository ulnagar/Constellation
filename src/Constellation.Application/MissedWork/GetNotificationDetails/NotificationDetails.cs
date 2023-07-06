namespace Constellation.Application.MissedWork.GetNotificationDetails;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record NotificationDetails(
    ClassworkNotificationId NotificationId,
    string OfferingName,
    DateOnly AbsenceDate,
    List<Name> Students,
    string Description);