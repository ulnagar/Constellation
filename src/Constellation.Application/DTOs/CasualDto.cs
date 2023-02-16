using Constellation.Application.Helpers;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class CasualDto
    {
        public int Id { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.FirstName)]
        public string FirstName { get; set; }
        [Required]
        [Display(Name=DisplayNameDefaults.LastName)]
        public string LastName { get; set; }
        [Required]
        [Display(Name = DisplayNameDefaults.PortalUsername)]
        public string PortalUsername { get; set; }
        [Display(Name=DisplayNameDefaults.IsDeleted)]
        public bool IsDeleted { get; set; }
        [DataType(DataType.Date)]
        [Display(Name =DisplayNameDefaults.DateDeleted)]
        public DateTime? DateDeleted { get; set; }
        [Display(Name=DisplayNameDefaults.AdobeConnectId)]
        public string AdobeConnectPrincipalId { get; set; }
        [Required]
        [Display(Name = DisplayNameDefaults.SchoolName)]
        public string SchoolCode { get; set; }
    }
}
