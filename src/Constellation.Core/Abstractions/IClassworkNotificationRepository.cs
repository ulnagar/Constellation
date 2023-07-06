namespace Constellation.Core.Abstractions;

using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.MissedWork;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IClassworkNotificationRepository
{
    Task<ClassworkNotification?> GetById(ClassworkNotificationId notificationId, CancellationToken cancellationToken = default);
    Task<List<ClassworkNotification>> GetForTeacher(string staffId, CancellationToken cancellationToken = default);
    Task<List<ClassworkNotification>> GetForOfferingAndDate(int offeringId, DateOnly absenceDate, CancellationToken cancellationToken = default);
    Task<List<ClassworkNotification>> GetOutstandingForStudent(string studentId, CancellationToken cancellationToken = default);
    Task<List<ClassworkNotification>> GetOutstandingForTeacher(string staffId, CancellationToken cancellationToken = default);


    void Insert(ClassworkNotification record);
}