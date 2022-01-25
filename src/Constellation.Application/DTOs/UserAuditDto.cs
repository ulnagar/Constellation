using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class UserAuditDto
    {
        public UserAuditDto()
        {
            Roles = new List<SchoolContactRole>();
        }

        [Display(Name="Contact")]
        public bool ContactPresent { get; set; }
        public SchoolContact Contact { get; set; }

        [Display(Name = "Role")]
        public bool RolesPresent { get; set; }
        public ICollection<SchoolContactRole> Roles { get; set; }

        [Display(Name = "User")]
        public bool UserPresent { get; set; }
        public AppUser User { get; set; }

        [Display(Name = "User Properties")]
        public bool UserPropertiesPresent { get; set; }
        [Display(Name = "Contact Link")]
        public bool UserContactLinkPresent { get; set; }

        [Display(Name = "User Role")]
        public bool UserRolePresent { get; set; }
    }
}
