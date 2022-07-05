using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Awards.Queries
{
    public class GetAwardsForStudentQueryHandler : IRequestHandler<GetAwardsForStudentQuery, ICollection<StudentAwardsForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAwardsForStudentQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentAwardsForList>> Handle(GetAwardsForStudentQuery request, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Where(student => student.StudentId == request.StudentId)
                .Select(student => student.Awards)
                .ProjectTo<ICollection<StudentAwardsForList>>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}