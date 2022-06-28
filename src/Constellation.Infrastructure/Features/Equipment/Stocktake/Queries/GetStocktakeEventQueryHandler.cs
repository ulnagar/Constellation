using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Equipment.Stocktake.Queries
{
    public class GetStocktakeEventQueryHandler : IRequestHandler<GetStocktakeEventQuery, StocktakeEvent>
    {
        private readonly IAppDbContext _context;

        public GetStocktakeEventQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<StocktakeEvent> Handle(GetStocktakeEventQuery request, CancellationToken cancellationToken)
        {
            if (request.IncludeSightings)
            {
                return await _context.StocktakeEvents
                    .Include(stocktake => stocktake.Sightings)
                    .FirstOrDefaultAsync(stocktake => stocktake.Id == request.StocktakeId, cancellationToken);
            }

            return await _context.StocktakeEvents
                .FirstOrDefaultAsync(stocktake => stocktake.Id == request.StocktakeId, cancellationToken);
        }
    }
}
