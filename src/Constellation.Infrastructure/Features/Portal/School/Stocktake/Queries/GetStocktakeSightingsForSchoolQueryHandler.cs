using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Stocktake.Models;
using Constellation.Application.Features.Portal.School.Stocktake.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Stocktake.Queries
{
    public class GetStocktakeSightingsForSchoolQueryHandler : IRequestHandler<GetStocktakeSightingsForSchoolQuery, ICollection<StocktakeSightingsForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStocktakeSightingsForSchoolQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StocktakeSightingsForList>> Handle(GetStocktakeSightingsForSchoolQuery request, CancellationToken cancellationToken)
        {
            return await _context.StocktakeSightings
                .Where(sighting => sighting.StocktakeEventId == request.StocktakeEvent && sighting.LocationCode == request.SchoolCode)
                .ProjectTo<StocktakeSightingsForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
