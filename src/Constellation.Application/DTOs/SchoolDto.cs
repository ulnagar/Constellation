using System.ComponentModel.DataAnnotations;

namespace Constellation.Application.DTOs
{
    public class SchoolDto
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        public string Address { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }

        [Required]
        public string EmailAddress { get; set; }
        public string Division { get; set; }
        public bool HeatSchool { get; set; }
        public string Electorate { get; set; }
        public string PrincipalNetwork { get; set; }
        public string TimetableApplication { get; set; }
        public string RollCallGroup { get; set; }
    }
}
