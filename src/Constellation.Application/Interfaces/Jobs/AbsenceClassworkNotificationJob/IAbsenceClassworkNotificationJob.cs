namespace Constellation.Application.Interfaces.Jobs.AbsenceClassworkNotificationJob;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceClassworkNotificationJob
{
    Task StartJob(Guid jobId, DateOnly scanDate, CancellationToken token);
}
