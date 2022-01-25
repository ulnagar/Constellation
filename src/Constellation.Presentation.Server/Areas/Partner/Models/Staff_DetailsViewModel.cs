using Constellation.Application.Extensions;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Staff_DetailsViewModel : BaseViewModel
    {
        public Staff_DetailsViewModel()
        {
            Offerings = new List<OfferingDto>();
            Sessions = new List<SessionDto>();
            SchoolStaff = new List<ContactDto>();
        }

        public StaffDto Staff { get; set; }

        public ICollection<OfferingDto> Offerings { get; set; }
        public ICollection<SessionDto> Sessions { get; set; }
        public ICollection<ContactDto> SchoolStaff { get; set; }

        public class StaffDto
        {
            public StaffDto()
            {
                Faculty = new List<string>();
            }

            [Display(Name=DisplayNameDefaults.StaffId)]
            public string StaffId { get; set; }
            [Display(Name=DisplayNameDefaults.IsDeleted)]
            public bool IsDeleted { get; set; }
            public string Name { get; set; }
            [Display(Name = DisplayNameDefaults.DateEntered)]
            public DateTime? DateEntered { get; set; }
            [Display(Name = DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name = DisplayNameDefaults.DateDeleted)]
            public DateTime? DateDeleted { get; set; }
            [Display(Name = DisplayNameDefaults.SchoolName)]
            public string SchoolName { get; set; }
            [Display(Name = DisplayNameDefaults.AdobeConnectId)]
            public string AdobeConnectId { get; set; }
            public ICollection<string> Faculty { get; set; }

            public static StaffDto ConvertFromStaff(Staff staff)
            {
                var viewModel = new StaffDto
                {
                    StaffId = staff.StaffId,
                    IsDeleted = staff.IsDeleted,
                    Name = staff.DisplayName,
                    DateDeleted = staff.DateDeleted,
                    DateEntered = staff.DateEntered,
                    EmailAddress = staff.EmailAddress,
                    SchoolName = staff.School.Name,
                    AdobeConnectId = staff.AdobeConnectPrincipalId,
                    Faculty = staff.Faculty.AsList()
                };

                return viewModel;
            }
        }

        public class OfferingDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [Display(Name = DisplayNameDefaults.CourseName)]
            public string CourseName { get; set; }

            public static OfferingDto ConvertFromOffering(CourseOffering offering)
            {
                var viewModel = new OfferingDto
                {
                    Id = offering.Id,
                    Name = offering.Name,
                    CourseName = offering.Course.Name
                };

                return viewModel;
            }
        }

        public class SessionDto
        {
            public TimetablePeriod Period { get; set; }
            [Display(Name = DisplayNameDefaults.ClassName)]
            public string ClassName { get; set; }
            [Display(Name = DisplayNameDefaults.RoomName)]
            public string RoomName { get; set; }
            public int Duration { get; set; }

            public static SessionDto ConvertFromSession(OfferingSession session)
            {
                var viewModel = new SessionDto
                {
                    Period = session.Period,
                    ClassName = session.Offering.Name,
                    RoomName = session.Room.Name
                };

                viewModel.Duration = session.Period.EndTime.Subtract(session.Period.StartTime).Minutes;
                viewModel.Duration += session.Period.EndTime.Subtract(session.Period.StartTime).Hours * 60;

                return viewModel;
            }
        }

        public class ContactDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            [Display(Name = DisplayNameDefaults.EmailAddress)]
            public string EmailAddress { get; set; }
            [Display(Name = DisplayNameDefaults.PhoneNumber)]
            public string PhoneNumber { get; set; }
            public string Role { get; set; }

            public static ContactDto ConvertFromRoleAssignment(SchoolContactRole role)
            {
                var viewModel = new ContactDto
                {
                    Id = role.Id,
                    Name = role.SchoolContact.DisplayName,
                    EmailAddress = role.SchoolContact.EmailAddress,
                    PhoneNumber = string.IsNullOrWhiteSpace(role.SchoolContact.PhoneNumber) ? role.School.PhoneNumber : role.SchoolContact.PhoneNumber,
                    Role = role.Role
                };

                return viewModel;
            }
        }
    }
}