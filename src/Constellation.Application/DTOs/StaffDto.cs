using Constellation.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class StaffDto
    {
        [Required]
        public string StaffId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        // Not required by highly desirable
        public string PortalUsername { get; set; }
        public string AdobeConnectPrincipalId { get; set; }

        [Required]
        public string SchoolCode { get; set; }
        [Required]
        public Faculty Faculty { get; set; }
    }
}
