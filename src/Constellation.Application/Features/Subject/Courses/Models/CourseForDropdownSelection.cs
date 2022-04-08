using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

namespace Constellation.Application.Features.Subject.Courses.Models
{
    public class CourseForDropdownSelection : IMapFrom<Course>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Faculty { get; set; }

        public string DisplayName => $"{Grade} {Name}";
    }
}
