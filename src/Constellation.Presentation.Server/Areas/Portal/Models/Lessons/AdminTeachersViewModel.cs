using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.BaseModels;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    using Core.Models.SchoolContacts.Identifiers;

    public class AdminTeachersViewModel : BaseViewModel
    {
        public AdminTeachersViewModel()
        {
            Teachers = new List<TeacherDto>();
        }

        public ICollection<TeacherDto> Teachers { get; set; }

        public class TeacherDto
        {
            public SchoolContactId Id { get; set; }
            public SchoolContactRoleId RoleId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public bool SelfRegistered { get; set; }
            public string SchoolCode { get; set; }
            public string SchoolName { get; set; }

            public static TeacherDto ConvertFromContactRole(SchoolContact contact, SchoolContactRole role)
            {
                var viewModel = new TeacherDto
                {
                    Id = contact.Id,
                    RoleId = role.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    EmailAddress = contact.EmailAddress,
                    PhoneNumber = contact.PhoneNumber,
                    SelfRegistered = contact.SelfRegistered,
                    SchoolCode = role.SchoolCode,
                    SchoolName = role.SchoolName
                };

                return viewModel;
            }
        }
    }
}