using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Assignments;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries
{
    public class GetAssignmentQueryHandler : IRequestHandler<GetAssignmentQuery, AssignmentForList>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GetAssignmentQueryHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AssignmentForList> Handle(GetAssignmentQuery request, CancellationToken cancellationToken)
        {
            return await _context.Set<CanvasAssignment>()
                .Where(assignment => assignment.Id.Value == request.Id)
                .ProjectTo<AssignmentForList>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
