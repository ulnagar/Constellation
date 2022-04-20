using Constellation.Application.DTOs;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Staff_UpdateViewModel : BaseViewModel
    {
        // Staff Object
        public LocalStaffDto Staff { get; set; }

        // View Properties
        public bool IsNew { get; set; }
        public SelectList SchoolList { get; set; }

        public Staff_UpdateViewModel()
        {
            Staff = new LocalStaffDto();
        }

        public class LocalStaffDto : StaffDto
        {
            [Required]
            [BindProperty(BinderType = typeof(FlagEnumModelBinder))]
            public override Faculty Faculty { get; set; }
        }
    }
}