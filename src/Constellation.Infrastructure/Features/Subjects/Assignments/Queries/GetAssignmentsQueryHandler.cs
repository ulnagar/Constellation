namespace Constellation.Infrastructure.Features.Subjects.Assignments.Queries;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Core.Models.Assignments;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;

public class GetAssignmentsQueryHandler : IRequestHandler<GetAssignmentsQuery, ICollection<AssignmentForList>>
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public GetAssignmentsQueryHandler(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ICollection<AssignmentForList>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Set<CanvasAssignment>()
            .Where(assignment => (assignment.DueDate >= DateTime.Today || (assignment.LockDate.HasValue ? assignment.LockDate.Value >= DateTime.Today : true)) && 
                (assignment.UnlockDate.HasValue ? assignment.UnlockDate.Value <= DateTime.Today : true))
            .ProjectTo<AssignmentForList>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
