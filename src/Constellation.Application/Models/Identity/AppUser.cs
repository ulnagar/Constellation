using Microsoft.AspNetCore.Identity;
using System;

namespace Constellation.Application.Models.Identity
{
    public class AppUser : IdentityUser<Guid>
    {
        public const string IsContact = "IsContact";
        public const string ContactId = "ContactId";
        public const string IsStaff = "IsStaff";
        public const string StaffMemberId = "StaffMemberId";

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName => FirstName + " " + LastName;
        public bool IsSchoolContact { get; set; }
        public int SchoolContactId { get; set; }
        public bool IsStaffMember { get; set; }
        public string StaffId { get; set; }
        public bool IsParent { get; set; }
    }
}
