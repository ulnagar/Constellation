namespace Constellation.Application.Interfaces.Jobs;

using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAbsenceProcessingJob
{
    Task<List<Absence>> StartJob(Guid jobId, Student student, CancellationToken cancellationToken);
}
