using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.ScienceRolls.Models;
using Constellation.Application.Features.Portal.School.ScienceRolls.Queries;
using Constellation.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.ScienceRolls.Queries
{
    public class GetScienceLessonRollForDisplayQueryHandler : MediatR.IRequestHandler<GetScienceLessonRollForDisplayQuery, ScienceLessonRollForDetails>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetScienceLessonRollForDisplayQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ScienceLessonRollForDetails> Handle(GetScienceLessonRollForDisplayQuery request, CancellationToken cancellationToken)
        {
            return await _context.LessonRolls
                .Where(roll => roll.Id == request.RollId)
                .ProjectTo<ScienceLessonRollForDetails>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
