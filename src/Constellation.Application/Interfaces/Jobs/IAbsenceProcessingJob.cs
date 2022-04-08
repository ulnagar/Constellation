using Constellation.Application.Features.Jobs.AbsenceMonitor.Models;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Jobs
{
    public interface IAbsenceProcessingJob : IHangfireJob
    {
        Task<ICollection<Absence>> StartJob(StudentForAbsenceScan student);
    }
}
