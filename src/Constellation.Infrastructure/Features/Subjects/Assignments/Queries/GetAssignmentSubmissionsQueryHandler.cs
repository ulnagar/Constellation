using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries
{
    public class GetAssignmentSubmissionsQueryHandler : IRequestHandler<GetAssignmentSubmissionsQuery, ICollection<AssignmentSubmissionForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAssignmentSubmissionsQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<AssignmentSubmissionForList>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
        {
            return await _context.CanvasAssignmentsSubmissions
                .Where(submission => submission.AssignmentId == request.Id)
                .ProjectTo<AssignmentSubmissionForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
