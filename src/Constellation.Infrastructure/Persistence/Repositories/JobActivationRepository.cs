using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.Repositories
{
    public class JobActivationRepository : IJobActivationRepository
    {
        private readonly AppDbContext _context;

        public JobActivationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<JobActivation>> GetAll()
        {
            return await _context.JobActivations
                .ToListAsync();
        }

        public async Task<JobActivation> GetForJob(string jobName)
        {
            return await _context.JobActivations
                .SingleOrDefaultAsync(job => job.JobName == jobName);
        }

        public async Task<JobActivation> GetFromId(Guid id)
        {
            return await _context.JobActivations
                .SingleOrDefaultAsync(job => job.Id == id);
        }
    }
}
