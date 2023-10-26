using Constellation.Application.Common.Mapping;
using Constellation.Core.Models.Students;

namespace Constellation.Application.Features.Portal.School.Home.Models
{
    public class StudentFromSchoolForDropdownSelection : IMapFrom<Student>
    {
        public string StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrentGrade { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
    }
}
