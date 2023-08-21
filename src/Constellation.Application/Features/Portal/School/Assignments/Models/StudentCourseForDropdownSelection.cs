using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Subjects;

namespace Constellation.Application.Features.Portal.School.Assignments.Models
{
    public class StudentCourseForDropdownSelection : IMapFrom<Course>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Grade { get; set; }
        public string DisplayName => $"{Grade} {Name}";
    }
}
