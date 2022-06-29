using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Stocktake.Models;
using Constellation.Application.Features.Portal.School.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Stocktake.Queries
{
    public class GetCurrentStocktakeEventsQueryHandler : IRequestHandler<GetCurrentStocktakeEventsQuery, ICollection<StocktakeEventsForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetCurrentStocktakeEventsQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StocktakeEventsForList>> Handle(GetCurrentStocktakeEventsQuery request, CancellationToken cancellationToken)
        {
            return await _context.StocktakeEvents
                .Where(stocktake => stocktake.EndDate.Date >= DateTime.Today && stocktake.StartDate.Date <= DateTime.Today)
                .ProjectTo<StocktakeEventsForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
