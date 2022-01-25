using Constellation.Application.Extensions;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Staff_ViewModel : BaseViewModel
    {
        public ICollection<StaffDto> Staff { get; set; }

        public class StaffDto
        {
            public StaffDto()
            {
                Faculty = new List<string>();
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public ICollection<string> Faculty { get; set; }
            public string SchoolName { get; set; }

            public static StaffDto ConvertFromStaff(Staff staff)
            {
                var viewModel = new StaffDto
                {
                    Id = staff.StaffId,
                    Name = staff.DisplayName,
                    Faculty = staff.Faculty.AsList(),
                    SchoolName = staff.School.Name
                };

                return viewModel;
            }
        }
    }
}