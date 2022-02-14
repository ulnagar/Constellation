using System;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IAbsenceClassworkNotificationJob
    {
        Task StartJob(DateTime scanDate);
    }
}
