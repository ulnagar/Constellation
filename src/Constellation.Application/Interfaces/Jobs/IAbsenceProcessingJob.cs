using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IAbsenceProcessingJob
    {
        Task<ICollection<Absence>> StartJob(Guid jobId, StudentForAbsenceScan student, CancellationToken cancellationToken);
    }
}
