using Constellation.Core.Models.SchoolContacts;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class SchoolStaff_WithRoleViewModel
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name => FirstName + " " + LastName;
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string SchoolCode { get; set; }
        public string SchoolName { get; set; }

        public bool StaffPhone { get; set; }

        public static SchoolStaff_WithRoleViewModel ConvertFromSchoolStaff(SchoolContact schoolContact)
        {
            var viewModel = new SchoolStaff_WithRoleViewModel
            {
                Id = schoolContact.Id,
                FirstName = schoolContact.FirstName,
                LastName = schoolContact.LastName,
                PhoneNumber = schoolContact.PhoneNumber,
                StaffPhone = true,
                EmailAddress = schoolContact.EmailAddress,
                Role = "",
                SchoolCode = "",
                SchoolName = ""
            };

            return viewModel;
        }

        public static SchoolStaff_WithRoleViewModel ConvertFromAssignment(SchoolContactRole assignment)
        {
            var staffPhone = !string.IsNullOrEmpty(assignment.SchoolContact.PhoneNumber);

            var viewModel = new SchoolStaff_WithRoleViewModel
            {
                Id = assignment.SchoolContact.Id,
                AssignmentId = assignment.Id,
                FirstName = assignment.SchoolContact.FirstName,
                LastName = assignment.SchoolContact.LastName,
                PhoneNumber = staffPhone ? assignment.SchoolContact.PhoneNumber : assignment.School.PhoneNumber,
                StaffPhone = staffPhone,
                EmailAddress = assignment.SchoolContact.EmailAddress,
                Role = assignment.Role,
                SchoolCode = assignment.School.Code,
                SchoolName = assignment.School.Name
            };

            return viewModel;
        }
    }
}