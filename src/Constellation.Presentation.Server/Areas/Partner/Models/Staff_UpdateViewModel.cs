using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Staff_UpdateViewModel : BaseViewModel
    {
        // Staff Object
        public StaffDto Staff { get; set; }

        // View Properties
        public bool IsNew { get; set; }
        public SelectList SchoolList { get; set; }

        public Staff_UpdateViewModel()
        {
            Staff = new StaffDto();
        }
    }
}