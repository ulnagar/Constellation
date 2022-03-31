using System.Collections.Generic;

namespace Constellation.Application.DTOs
{
    public class FamilyDetailsDto
    {
        public List<string> StudentIds { get; set; } = new List<string>();
        public string FamilyId { get; set; }
        public string FatherTitle { get; set; }
        public string FatherFirstName { get; set; }
        public string FatherLastName { get; set; }
        public string FatherMobile { get; set; }
        public string MotherTitle { get; set; }
        public string MotherFirstName { get; set; }
        public string MotherLastName { get; set; }
        public string MotherMobile { get; set; }
        public string FamilyEmail { get; set; }
        public string AddressName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressTown { get; set; }
        public string AddressPostCode { get; set; }
    }
}
