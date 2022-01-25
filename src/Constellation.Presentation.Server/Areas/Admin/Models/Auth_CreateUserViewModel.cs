using Constellation.Presentation.Server.BaseModels;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Auth_CreateUserViewModel : BaseViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public bool CoverEmails { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirm password fields do not match!")]
        public string ConfirmPassword { get; set; }
    }
}