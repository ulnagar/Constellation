using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Admin.HangfireDashboard.Queries
{
    public class GetJobActivatorRecordsQuery : IRequest<ICollection<JobActivation>>
    {
    }

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
