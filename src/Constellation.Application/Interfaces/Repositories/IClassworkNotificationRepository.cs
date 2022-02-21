using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IClassworkNotificationRepository
    {
        Task<ICollection<ClassworkNotification>> GetAll();
        Task<ClassworkNotification> Get(Guid id);
        Task<ClassworkNotification> GetForDuplicateCheck(int offeringId, DateTime absenceDate);
        Task<ICollection<ClassworkNotification>> GetOutstandingForTeacher(string staffId);
    }
}
