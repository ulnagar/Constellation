using Constellation.Core.Models.SchoolContacts;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    using Core.Models.SchoolContacts.Identifiers;

    public class SchoolStaff_WithRoleViewModel
    {
        public SchoolContactId Id { get; set; }
        public SchoolContactRoleId AssignmentId { get; set; }
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

        public static SchoolStaff_WithRoleViewModel ConvertFromAssignment(SchoolContact contact, SchoolContactRole assignment)
        {
            var viewModel = new SchoolStaff_WithRoleViewModel
            {
                Id = contact.Id,
                AssignmentId = assignment.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                PhoneNumber = contact.PhoneNumber,
                EmailAddress = contact.EmailAddress,
                Role = assignment.Role,
                SchoolCode = assignment.SchoolCode,
                SchoolName = assignment.SchoolName
            };

            return viewModel;
        }
    }
}