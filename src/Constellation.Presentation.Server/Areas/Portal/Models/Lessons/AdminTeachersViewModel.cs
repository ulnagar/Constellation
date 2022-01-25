using Constellation.Core.Models;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminTeachersViewModel
    {
        public AdminTeachersViewModel()
        {
            Teachers = new List<TeacherDto>();
        }

        public ICollection<TeacherDto> Teachers { get; set; }

        public class TeacherDto
        {
            public int Id { get; set; }
            public int RoleId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EmailAddress { get; set; }
            public string PhoneNumber { get; set; }
            public bool SelfRegistered { get; set; }
            public string SchoolCode { get; set; }
            public string SchoolName { get; set; }

            public static TeacherDto ConvertFromContactRole(SchoolContactRole role)
            {
                var viewModel = new TeacherDto
                {
                    Id = role.SchoolContactId,
                    RoleId = role.Id,
                    FirstName = role.SchoolContact.FirstName,
                    LastName = role.SchoolContact.LastName,
                    EmailAddress = role.SchoolContact.EmailAddress,
                    PhoneNumber = role.SchoolContact.PhoneNumber,
                    SelfRegistered = role.SchoolContact.SelfRegistered,
                    SchoolCode = role.SchoolCode,
                    SchoolName = role.School.Name
                };

                return viewModel;
            }
        }
    }
}