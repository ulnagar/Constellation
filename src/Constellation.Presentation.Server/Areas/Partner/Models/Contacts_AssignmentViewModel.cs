using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Contacts_AssignmentViewModel : BaseViewModel
    {
        public SchoolContactRoleDto ContactRole { get; set; }
        public SelectList SchoolStaffList { get; set; }
        public SelectList RoleList { get; set; }
        public SelectList SchoolList { get; set; }
        public string ReturnUrl { get; set; }
    }
}