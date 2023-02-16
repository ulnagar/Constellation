using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminUpsertTeacherViewModel : BaseViewModel
    {
        public int? Id { get; set; }
        public int? RoleId { get; set; }
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

        public static AdminUpsertTeacherViewModel ConvertFromContactRole(SchoolContactRole role)
        {
            var viewModel = new AdminUpsertTeacherViewModel
            {
                Id = role.SchoolContactId,
                RoleId = role.Id,
                FirstName = role.SchoolContact.FirstName,
                LastName = role.SchoolContact.LastName,
                PhoneNumber = role.SchoolContact.PhoneNumber,
                EmailAddress = role.SchoolContact.EmailAddress,
                SchoolCode = role.SchoolCode
            };

            return viewModel;
        }
    }
}