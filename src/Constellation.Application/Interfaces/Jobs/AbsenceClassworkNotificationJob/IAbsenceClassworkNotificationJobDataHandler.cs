﻿using Constellation.Application.Models.EmailQueue;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;

public interface IAbsenceClassworkNotificationJobDataHandler
{
    Task<ClassworkNotification?> GetClassworkNotification(DateTime absenceDate, int offeringId, CancellationToken token);
    Task<List<ClassCover>> GetCovers(DateTime absenceDate, int offeringId, CancellationToken token);
    Task<List<string>> GetParentEmailAddresses(Absence absence, CancellationToken token);
    Task<List<StudentFamily>> GetFamilies(string studentId, CancellationToken token);
    Task<ICollection<Absence>> GetAbsences(DateTime scanDate, CancellationToken token);
    Task SaveClassworkNotification(ClassworkNotification notification, CancellationToken token);
    Task SaveEmail(EmailQueueItem item, CancellationToken token);
    Task SaveAbsenceToNotification(Guid notificationId, Absence absence, CancellationToken token);
}