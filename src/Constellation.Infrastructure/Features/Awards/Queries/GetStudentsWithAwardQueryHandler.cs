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
    public class GetStudentsWithAwardQueryHandler : IRequestHandler<GetStudentsWithAwardQuery, ICollection<StudentWithAwards>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentsWithAwardQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<StudentWithAwards>> Handle(GetStudentsWithAwardQuery request, CancellationToken cancellationToken)
        {
            return await _context.Students
                .Include(student => student.Awards)
                .Where(student => !student.IsDeleted)
                .ProjectTo<StudentWithAwards>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
