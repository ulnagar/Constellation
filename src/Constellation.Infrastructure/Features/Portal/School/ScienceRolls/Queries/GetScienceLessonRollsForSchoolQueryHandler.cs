using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using Constellation.Application.Features.Portal.School.ScienceRolls.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.ScienceRolls.Queries
{
    public class GetScienceLessonRollsForSchoolQueryHandler : IRequestHandler<GetScienceLessonRollsForSchoolQuery, ICollection<ScienceLessonRollForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetScienceLessonRollsForSchoolQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<ScienceLessonRollForList>> Handle(GetScienceLessonRollsForSchoolQuery request, CancellationToken cancellationToken)
        {
            return await _context.LessonRolls
                .Where(roll => roll.SchoolCode == request.SchoolCode && roll.Lesson.DueDate.Year == DateTime.Now.Year)
                .ProjectTo<ScienceLessonRollForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
