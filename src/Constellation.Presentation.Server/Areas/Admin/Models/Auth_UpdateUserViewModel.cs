using Constellation.Presentation.Server.BaseModels;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Admin.Models
{
    public class Auth_UpdateUserViewModel : BaseViewModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}