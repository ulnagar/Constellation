using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries
{
    public class GetAssignmentQueryHandler : IRequestHandler<GetAssignmentQuery, AssignmentForList>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAssignmentQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AssignmentForList> Handle(GetAssignmentQuery request, CancellationToken cancellationToken)
        {
            return await _context.CanvasAssignments
                .Where(assignment => assignment.Id == request.Id)
                .ProjectTo<AssignmentForList>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
