using Constellation.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IJobActivationRepository
    {
        Task<ICollection<JobActivation>> GetAll();
        Task<JobActivation> GetForJob(string jobName);
        Task<JobActivation> GetFromId(Guid id);
    }
}
