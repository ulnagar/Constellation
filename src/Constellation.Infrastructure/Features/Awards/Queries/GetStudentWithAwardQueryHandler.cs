using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Awards.Models;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Awards.Queries
{
    public class GetStudentWithAwardQueryHandler : IRequestHandler<GetStudentWithAwardQuery, StudentWithAwards>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetStudentWithAwardQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StudentWithAwards> Handle(GetStudentWithAwardQuery request, CancellationToken cancellationToken)
        {
            return await _context.Students
                //.Include(student => student.Awards)
                .Where(student => student.StudentId == request.StudentId)
                .ProjectTo<StudentWithAwards>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
