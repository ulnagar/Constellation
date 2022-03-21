using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Common.Mapping;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Subject.Assignments.Queries
{
    public class GetAssignmentsQuery : IRequest<ICollection<AssignmentForList>>
    {
    }

    public class AssignmentForList : IMapFrom<CanvasAssignment>
    {
        public Guid Id { get; set; }
        public string CourseName { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? LockDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public int AllowedAttempts { get; set; }
    }

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
                .Where(assignment => assignment.DueDate >= DateTime.Today && (assignment.UnlockDate.HasValue ? assignment.UnlockDate.Value <= DateTime.Today : true))
                .ProjectTo<AssignmentForList>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
