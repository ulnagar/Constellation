using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class SchoolStaff_UpdateViewModel : BaseViewModel
    {
        public SchoolContactDto Contact { get; set; }
        public bool IsNew { get; set; }
        public SchoolContactRoleDto ContactRole { get; set; } = new();
        public SelectList RoleList { get; set; }
        public SelectList SchoolList { get; set; }
    }
}