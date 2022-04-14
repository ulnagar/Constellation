using Constellation.Application.Features.ShortTerm.Covers.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.ShortTerm.Covers.Queries
{
    public class GetTeamLinkForOfferingQueryHandler : IRequestHandler<GetTeamLinkForOfferingQuery, string>
    {
        private readonly IAppDbContext _context;

        public GetTeamLinkForOfferingQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(GetTeamLinkForOfferingQuery request, CancellationToken cancellationToken)
        {
            return await _context.Teams
                .Where(team => team.Name.Contains(request.ClassName) && team.Name.Contains(request.Year))
                .Select(team => team.Link)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
