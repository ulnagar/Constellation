using Constellation.Application.Features.Subject.Courses.Models;
using MediatR;
using System.Collections.Generic;

namespace Constellation.Application.Features.Subject.Courses.Queries
{
    public class GetCoursesForDropdownSelectionQuery : IRequest<ICollection<CourseForDropdownSelection>>
    {
    }
}
