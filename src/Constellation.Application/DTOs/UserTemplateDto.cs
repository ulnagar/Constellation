namespace Constellation.Application.DTOs
{
    using Core.Models.SchoolContacts.Identifiers;

    public class UserTemplateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? IsSchoolContact { get; set; }
        public SchoolContactId SchoolContactId { get; set; }
        public bool? IsStaffMember { get; set; }
        public string StaffId { get; set; }
    }
}
