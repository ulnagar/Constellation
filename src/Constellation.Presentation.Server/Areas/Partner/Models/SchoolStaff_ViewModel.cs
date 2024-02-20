using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class SchoolStaff_ViewModel : BaseViewModel
    {
        public SchoolStaff_ViewModel()
        {
            Contacts = new List<ContactDto>();
        }

        public ICollection<ContactDto> Contacts { get; set; }
        public ICollection<string> RoleList { get; set; }

        public class ContactDto
        {
            public int Id { get; set; }
            public int AssignmentId { get; set; }
            public string Name { get;set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
            public string SchoolName { get; set; }

            public static ContactDto ConvertFromContact(SchoolContact contact)
            {
                var viewModel = new ContactDto
                {
                    Id = contact.Id,
                    AssignmentId = 0,
                    Name = contact.DisplayName,
                    EmailAddress = contact.EmailAddress,
                    PhoneNumber = contact.PhoneNumber,
                    Role = string.Empty,
                    SchoolName = string.Empty
                };

                return viewModel;
            }

            public static ICollection<ContactDto> ConvertFromContactWithRole(SchoolContact contact)
            {
                var contactList = new List<ContactDto>();

                foreach (var role in contact.Assignments.Where(role => !role.IsDeleted))
                {
                    var viewModel = new ContactDto
                    {
                        Id = contact.Id,
                        AssignmentId = role.Id,
                        Name = contact.DisplayName,
                        EmailAddress = contact.EmailAddress,
                        PhoneNumber = string.IsNullOrEmpty(contact.PhoneNumber) ? role.School.PhoneNumber : contact.PhoneNumber,
                        Role = role.Role,
                        SchoolName = role.School.Name
                    };

                    contactList.Add(viewModel);
                }

                return contactList;
            }

            public static ContactDto ConvertFromAssignment(SchoolContactRole role)
            {
                var viewModel = new ContactDto
                {
                    Id = role.SchoolContact.Id,
                    AssignmentId = role.Id,
                    Name = role.SchoolContact.DisplayName,
                    EmailAddress = role.SchoolContact.EmailAddress,
                    PhoneNumber = string.IsNullOrEmpty(role.SchoolContact.PhoneNumber) ? role.School.PhoneNumber : role.SchoolContact.PhoneNumber,
                    Role = role.Role,
                    SchoolName = role.School.Name
                };

                return viewModel;
            }
        }
    }
}