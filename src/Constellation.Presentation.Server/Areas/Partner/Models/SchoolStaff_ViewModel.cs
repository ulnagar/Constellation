using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;
using System.Linq;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    using Core.Models.SchoolContacts.Identifiers;

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
            public SchoolContactId Id { get; set; }
            public SchoolContactRoleId AssignmentId { get; set; }
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
                    AssignmentId = null,
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
                        PhoneNumber = contact.PhoneNumber,
                        Role = role.Role,
                        SchoolName = role.SchoolName
                    };

                    contactList.Add(viewModel);
                }

                return contactList;
            }

            public static ContactDto ConvertFromAssignment(SchoolContact contact, SchoolContactRole role)
            {
                var viewModel = new ContactDto
                {
                    Id = contact.Id,
                    AssignmentId = role.Id,
                    Name = contact.DisplayName,
                    EmailAddress = contact.EmailAddress,
                    PhoneNumber = contact.PhoneNumber,
                    Role = role.Role,
                    SchoolName = role.SchoolName
                };

                return viewModel;
            }
        }
    }
}