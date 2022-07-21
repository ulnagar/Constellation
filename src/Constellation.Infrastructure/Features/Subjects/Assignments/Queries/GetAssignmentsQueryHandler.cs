using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries
{
    public class GetAssignmentsQueryHandler : IRequestHandler<GetAssignmentsQuery, ICollection<AssignmentForList>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAssignmentsQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICollection<AssignmentForList>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
        {
            return await _context.CanvasAssignments
                .Where(assignment => (assignment.DueDate >= DateTime.Today || (assignment.LockDate.HasValue ? assignment.LockDate.Value >= DateTime.Today : true)) && 
                    (assignment.UnlockDate.HasValue ? assignment.UnlockDate.Value <= DateTime.Today : true))
                .ProjectTo<AssignmentForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
