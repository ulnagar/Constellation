namespace Constellation.Application.DTOs
{
    public class UserTemplateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? IsSchoolContact { get; set; }
        public int SchoolContactId { get; set; }
        public bool? IsStaffMember { get; set; }
        public string StaffId { get; set; }
    }
}
