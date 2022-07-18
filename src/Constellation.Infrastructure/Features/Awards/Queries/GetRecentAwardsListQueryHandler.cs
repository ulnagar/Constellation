using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Awards.Queries
{
    public class GetRecentAwardsListQueryHandler : IRequestHandler<GetRecentAwardsListQuery, ICollection<AwardWithStudentName>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetRecentAwardsListQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<AwardWithStudentName>> Handle(GetRecentAwardsListQuery request, CancellationToken cancellationToken)
        {
            var query = _context.StudentAward
                .OrderByDescending(award => award.AwardedOn);

            if (request.RecentCount > 0)
                query = (IOrderedQueryable<Core.Models.StudentAward>)query.Take(request.RecentCount);

            if (request.SinceDate != new DateOnly())
                query = (IOrderedQueryable<Core.Models.StudentAward>)query.Where(award => DateOnly.FromDateTime(award.AwardedOn) >= request.SinceDate);

            return await query.ProjectTo<AwardWithStudentName>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            //return await _context.StudentAward
            //    .OrderByDescending(award => award.AwardedOn)
            //    .Take(request.RecentCount)
            //    .ProjectTo<AwardWithStudentName>(_mapper.ConfigurationProvider)
            //    .ToListAsync(cancellationToken);
        }
    }
}
