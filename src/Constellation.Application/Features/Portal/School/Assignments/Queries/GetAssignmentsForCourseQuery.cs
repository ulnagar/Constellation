using Constellation.Application.Features.Portal.School.Assignments.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Assignments.Queries
{
    public class GetAssignmentsForCourseQuery : IRequest<ICollection<StudentAssignmentForCourse>>
    {
        public int CourseId { get; set; }
        public string StudentId { get; set; }
    }

}
