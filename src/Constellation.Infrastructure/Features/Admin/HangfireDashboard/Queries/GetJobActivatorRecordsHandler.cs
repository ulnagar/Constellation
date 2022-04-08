using Constellation.Application.Features.Admin.HangfireDashboard.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Admin.HangfireDashboard.Queries
{
    public class GetJobActivatorRecordsHandler : IRequestHandler<GetJobActivatorRecordsQuery, ICollection<JobActivation>>
    {
        private readonly IAppDbContext _context;

        public GetJobActivatorRecordsHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<JobActivation>> Handle(GetJobActivatorRecordsQuery request, CancellationToken cancellationToken)
        {
            var records = await _context.JobActivations.ToListAsync();

            return records;
        }
    }
}
