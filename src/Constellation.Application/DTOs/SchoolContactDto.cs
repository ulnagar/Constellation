using Constellation.Presentation.Server.Helpers.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class SchoolContactDto
    {
        public int? Id { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.FirstName)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.LastName)]
        public string LastName { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.EmailAddress)]
        public string EmailAddress { get; set; }
        [Display(Name=DisplayNameDefaults.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public bool SelfRegistered { get; set; }

        public SchoolContactDto()
        {
            Id = 0;
        }
    }
}
