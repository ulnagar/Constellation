namespace Constellation.Application.MissedWork.Models;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using System;
using System.Collections.Generic;

public sealed record NotificationSummary(
    ClassworkNotificationId NotificationId,
    string ClassName,
    DateOnly ClassDate,
    List<Name> Students,
    bool IsCompleted);