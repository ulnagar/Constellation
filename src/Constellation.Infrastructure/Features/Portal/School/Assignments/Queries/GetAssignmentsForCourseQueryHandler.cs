using AutoMapper;
using AutoMapper.QueryableExtensions;
using Constellation.Application.Features.Portal.School.Assignments.Models;
using Constellation.Application.Features.Portal.School.Assignments.Queries;
using Constellation.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Features.Portal.School.Assignments.Queries
{
    public class GetAssignmentsForCourseQueryHandler : IRequestHandler<GetAssignmentsForCourseQuery, ICollection<StudentAssignmentForCourse>>
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;

        public GetAssignmentsForCourseQueryHandler(IAppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ICollection<StudentAssignmentForCourse>> Handle(GetAssignmentsForCourseQuery request, CancellationToken cancellationToken)
        {
            var assignments = await _context.CanvasAssignments
                .Where(assignment =>
                    assignment.CourseId == request.CourseId &&
                    (assignment.AllowedAttempts <= 0 || _context.CanvasAssignmentsSubmissions.Count(submission => submission.AssignmentId == assignment.Id && submission.StudentId == request.StudentId) < assignment.AllowedAttempts))
                .ProjectTo<StudentAssignmentForCourse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return assignments;
        }
    }

}
