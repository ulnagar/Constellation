using Constellation.Application.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    using System;

    public class SchoolContactDto
    {
        public Guid? Id { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.FirstName)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [Display(Name = DisplayNameDefaults.LastName)]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [Display(Name = DisplayNameDefaults.EmailAddress)]
        public string EmailAddress { get; set; } = string.Empty;
        [Display(Name = DisplayNameDefaults.PhoneNumber)]
        public string PhoneNumber { get; set; } = string.Empty;
        public bool SelfRegistered { get; set; }
    }
}
