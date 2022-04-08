using Constellation.Application.Features.Portal.School.Assignments.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Portal.School.Assignments.Queries
{
    public class GetCoursesForStudentQuery : IRequest<ICollection<StudentCourseForDropdownSelection>>
    {
        public string StudentId { get; set; }
    }
}
