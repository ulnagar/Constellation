using Constellation.Application.Extensions;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Staff_ViewModel : BaseViewModel
    {
        public ICollection<StaffDto> Staff { get; set; }
        public IDictionary<Guid, string> FacultyList { get; set; } = new Dictionary<Guid, string>();


        public class StaffDto
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<Faculty> Faculties { get; set; } = new();
            public string SchoolName { get; set; }


            public static StaffDto ConvertFromStaff(Staff staff)
            {
                var viewModel = new StaffDto
                {
                    Id = staff.StaffId,
                    Name = staff.DisplayName,
                    Faculties = staff.Faculties.Where(member => !member.IsDeleted).Select(member => member.Faculty).ToList(),
                    SchoolName = staff.School.Name
                };

                return viewModel;
            }
        }
    }
}