using Constellation.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class StudentDto
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        // Not required but highly desirable
        public string PortalUsername { get; set; }
        public string AdobeConnectPrincipalId { get; set; }
        public string SentralStudentId { get; set; }

        [Required]
        public Grade EnrolledGrade { get; set; }
        public Grade CurrentGrade { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string SchoolCode { get; set; }
    }
}
