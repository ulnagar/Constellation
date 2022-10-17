namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IClassworkNotificationRepository
{
    Task<ICollection<ClassworkNotification>> GetAll(CancellationToken token = default);
    Task<ClassworkNotification> Get(Guid id, CancellationToken token = default);
    Task<ClassworkNotification> GetForDuplicateCheck(int offeringId, DateTime absenceDate, CancellationToken token = default);
    Task<ICollection<ClassworkNotification>> GetOutstandingForTeacher(string staffId, CancellationToken token = default);
    Task<ICollection<ClassworkNotification>> GetForTeacher(string staffId, CancellationToken token = default);
}
