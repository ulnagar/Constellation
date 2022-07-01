using Constellation.Application.Common.Mapping;
using Constellation.Core.Models;

namespace Constellation.Application.Features.Equipment.Stocktake.Models
{
    public class PartnerSchoolForDropdownSelection : IMapFrom<School>
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
