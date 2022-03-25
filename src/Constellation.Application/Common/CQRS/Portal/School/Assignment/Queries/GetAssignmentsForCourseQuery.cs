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

namespace Constellation.Application.Common.CQRS.Portal.School.Assignment.Queries
{
    public class GetAssignmentsForCourseQuery : IRequest<ICollection<StudentAssignmentForCourse>>
    {
        public int CourseId { get; set; }
        public string StudentId { get; set; }
    }

    public class StudentAssignmentForCourse : IMapFrom<CanvasAssignment>
    {
        public Guid Id { get; set; }
        public int CanvasId { get; set; }
        public string Name { get; set; }
        public DateTime DueDate { get; set; }
    }

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
