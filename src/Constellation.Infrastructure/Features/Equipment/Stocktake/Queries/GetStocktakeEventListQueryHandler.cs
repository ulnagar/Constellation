using Constellation.Application.Features.Equipment.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Stocktake;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Equipment.Stocktake.Queries
{
    public class GetStocktakeEventListQueryHandler : MediatR.IRequestHandler<GetStocktakeEventListQuery, ICollection<StocktakeEvent>>
    {
        private readonly IAppDbContext _context;

        public GetStocktakeEventListQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<StocktakeEvent>> Handle(GetStocktakeEventListQuery request, CancellationToken cancellationToken)
        {
            return await _context.StocktakeEvents
                .ToListAsync(cancellationToken);
        }
    }
}
