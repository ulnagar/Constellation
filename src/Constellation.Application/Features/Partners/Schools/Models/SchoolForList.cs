using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

namespace Constellation.Application.Features.Partners.Schools.Models
{
    public class SchoolForList : IMapFrom<School>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Town { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
    }
}
