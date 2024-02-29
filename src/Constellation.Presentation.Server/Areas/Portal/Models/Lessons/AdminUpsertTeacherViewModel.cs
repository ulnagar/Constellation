using Constellation.Application.Helpers;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    using Core.Models.SchoolContacts.Identifiers;

    public class AdminUpsertTeacherViewModel : BaseViewModel
    {
        public SchoolContactId? Id { get; set; }
        public SchoolContactRoleId? RoleId { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.FirstName)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.LastName)]
        public string LastName { get; set; }
        [Display(Name=DisplayNameDefaults.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.EmailAddress)]
        public string EmailAddress { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.School)]
        public string SchoolCode { get; set; }

        public SelectList SchoolList { get; set; }

        public static AdminUpsertTeacherViewModel ConvertFromContactRole(SchoolContact contact, SchoolContactRole role)
        {
            var viewModel = new AdminUpsertTeacherViewModel
            {
                Id = contact.Id,
                RoleId = role.Id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                PhoneNumber = contact.PhoneNumber,
                EmailAddress = contact.EmailAddress,
                SchoolCode = role.SchoolCode
            };

            return viewModel;
        }
    }
}