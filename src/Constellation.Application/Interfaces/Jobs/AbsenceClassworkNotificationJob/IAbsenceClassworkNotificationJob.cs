using System;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob
{
    public interface IAbsenceClassworkNotificationJob
    {
        Task StartJob(Guid jobId, DateTime scanDate, CancellationToken token);
    }
}
