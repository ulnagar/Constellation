namespace Constellation.Presentation.Server.Areas.Partner.Models;

using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.TeacherAddFaculty;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.ValueObjects;
using Core.Models.Offerings.ValueObjects;
using System.ComponentModel.DataAnnotations;

public class Staff_DetailsViewModel : BaseViewModel
{
    public StaffDto Staff { get; set; }

    public List<OfferingDto> Offerings { get; set; } = new();
    public List<SessionDto> Sessions { get; set; } = new();
    public List<ContactDto> SchoolStaff { get; set; } = new();
    public List<FacultyDto> Faculties { get; set; } = new();

    public TeacherAddFacultySelection FacultyAssignmentDto { get; set; } = new();

    public class StaffDto
    {
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
                AdobeConnectId = staff.AdobeConnectPrincipalId
            };

            return viewModel;
        }
    }

    public class OfferingDto
    {
        public OfferingId Id { get; set; }
        public string Name { get; set; }
        [Display(Name = DisplayNameDefaults.CourseName)]
        public string CourseName { get; set; }
        public AssignmentType AssignmentType { get; set; }
    }

    public class SessionDto
    {
        public string PeriodSortOrder { get; set; }
        public string Period { get; set; }
        [Display(Name = DisplayNameDefaults.ClassName)]
        public string ClassName { get; set; }
        [Display(Name = DisplayNameDefaults.RoomName)]
        public int Duration { get; set; }
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

    public sealed record FacultyDto(
        FacultyMembershipId Id,
        FacultyId FacultyId,
        string Name,
        FacultyMembershipRole Role);
}