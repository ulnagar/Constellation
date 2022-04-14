using Constellation.Application.Features.API.Operations.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.API.Operations.Queries
{
    public class GetTeamIdForOfferingQueryHandler : IRequestHandler<GetTeamIdForOfferingQuery, string>
    {
        private readonly IAppDbContext _context;

        public GetTeamIdForOfferingQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(GetTeamIdForOfferingQuery request, CancellationToken cancellationToken)
        {
            return await _context.Teams
                .Where(team => team.Name.Contains(request.ClassName) && team.Name.Contains(request.Year))
                .Select(team => team.Id.ToString())
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
