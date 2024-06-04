namespace Constellation.Presentation.Server.Areas.Partner.Models;

using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts.Identifiers;
using Presentation.Shared.Pages.Shared.Components.TeacherAddFaculty;
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
        public SchoolContactId ContactId { get; set; }
        public SchoolContactRoleId RoleId { get; set; }
        public string Name { get; set; }
        [Display(Name = DisplayNameDefaults.EmailAddress)]
        public string EmailAddress { get; set; }
        [Display(Name = DisplayNameDefaults.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        public static ContactDto ConvertFromRoleAssignment(SchoolContact contact, SchoolContactRole role) =>
            new()
            {
                ContactId = contact.Id,
                RoleId = role.Id,
                Name = contact.DisplayName,
                EmailAddress = contact.EmailAddress,
                PhoneNumber = contact.PhoneNumber,
                Role = role.Role
            };
    }

    public sealed record FacultyDto(
        FacultyMembershipId Id,
        FacultyId FacultyId,
        string Name,
        FacultyMembershipRole Role);
}