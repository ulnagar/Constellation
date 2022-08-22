using Constellation.Application.Common.Mapping;

namespace Constellation.Application.Features.Portal.School.Contacts.Models
{
    public class SchoolContactDetails : IMapFrom<Core.Models.School>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public string EmailAddress { get; set; }
    }
}
