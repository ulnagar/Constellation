﻿namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IClassworkNotificationRepository
{
    Task<ClassworkNotification?> GetById(ClassworkNotificationId notificationId, CancellationToken cancellationToken = default);
    Task<ClassworkNotification?> GetForOfferingAndDate(int offeringId, DateOnly absenceDate, CancellationToken cancellationToken = default);

    void Insert(ClassworkNotification record);
}