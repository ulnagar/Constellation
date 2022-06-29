using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

namespace Constellation.Application.Features.Portal.School.Home.Models
{
    public class StaffFromSchoolForDropdownSelection : IMapFrom<Staff>
    {
        public string StaffId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => $"{FirstName} {LastName}";
    }
}
